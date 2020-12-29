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
using Tweetinvi.Models;

namespace BirdsiteLive.Pipeline.Processors
{
    public class RetrieveTweetsProcessor : IRetrieveTweetsProcessor
    {
        private readonly ITwitterService _twitterService;
        private readonly ITwitterUserDal _twitterUserDal;

        #region Ctor
        public RetrieveTweetsProcessor(ITwitterService twitterService, ITwitterUserDal twitterUserDal)
        {
            _twitterService = twitterService;
            _twitterUserDal = twitterUserDal;
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
                    await _twitterUserDal.UpdateTwitterUserAsync(user.Id, tweetId, tweetId);
                }
            }

            return usersWtTweets.ToArray();
        }

        private ExtractedTweet[] RetrieveNewTweets(SyncTwitterUser user)
        {
            ExtractedTweet[] tweets;
            if (user.LastTweetPostedId == -1)
                tweets = _twitterService.GetTimeline(user.Acct, 1);
            else
                tweets = _twitterService.GetTimeline(user.Acct, 200, user.LastTweetSynchronizedForAllFollowersId);

            return tweets;
        }
    }
}