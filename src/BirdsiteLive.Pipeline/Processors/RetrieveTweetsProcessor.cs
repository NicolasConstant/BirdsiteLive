using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Twitter;
using BirdsiteLive.Twitter.Models;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;

namespace BirdsiteLive.Pipeline.Processors
{
    public class RetrieveTweetsProcessor : IRetrieveTweetsProcessor
    {
        private readonly ITwitterTweetsService _twitterTweetsService;
        private readonly ITwitterUserService _twitterUserService;
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly ILogger<RetrieveTweetsProcessor> _logger;

        #region Ctor
        public RetrieveTweetsProcessor(ITwitterTweetsService twitterTweetsService, ITwitterUserDal twitterUserDal, ITwitterUserService twitterUserService, ILogger<RetrieveTweetsProcessor> logger)
        {
            _twitterTweetsService = twitterTweetsService;
            _twitterUserDal = twitterUserDal;
            _twitterUserService = twitterUserService;
            _logger = logger;
        }
        #endregion

        public async Task<UserWithTweetsToSync[]> ProcessAsync(SyncTwitterUser[] syncTwitterUsers, CancellationToken ct)
        {
            var usersWtTweets = new List<UserWithTweetsToSync>();

            //TODO multithread this
            foreach (var user in syncTwitterUsers)
            {
                var tweets = RetrieveNewTweets(user);
                if (tweets.Length > 0 && user.LastTweetPostedId != -1)
                {
                    var userWtTweets = new UserWithTweetsToSync
                    {
                        User = user,
                        Tweets = tweets
                    };
                    usersWtTweets.Add(userWtTweets);
                }
                else if (tweets.Length > 0 && user.LastTweetPostedId == -1)
                {
                    var tweetId = tweets.Last().Id;
                    var now = DateTime.UtcNow;
                    await _twitterUserDal.UpdateTwitterUserAsync(user.Id, tweetId, tweetId, now);
                }
                else
                {
                    var now = DateTime.UtcNow;
                    await _twitterUserDal.UpdateTwitterUserAsync(user.Id, user.LastTweetPostedId, user.LastTweetSynchronizedForAllFollowersId, now);
                }
            }

            return usersWtTweets.ToArray();
        }

        private ExtractedTweet[] RetrieveNewTweets(SyncTwitterUser user)
        {
            var tweets = new ExtractedTweet[0];

            try
            {
                if (user.LastTweetPostedId == -1)
                    tweets = _twitterTweetsService.GetTimeline(user.Acct, 1);
                else
                    tweets = _twitterTweetsService.GetTimeline(user.Acct, 200, user.LastTweetSynchronizedForAllFollowersId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving TL of {Username} from {LastTweetPostedId}", user.Acct, user.LastTweetPostedId);

                if (_twitterUserService is CachedTwitterUserService service)
                {
                    _logger.LogInformation("Purge {Username} from cache", user.Acct);
                    service.PurgeUser(user.Acct);
                }
            }

            return tweets;
        }
    }
}