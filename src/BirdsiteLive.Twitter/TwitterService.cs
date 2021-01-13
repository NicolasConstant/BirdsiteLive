using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Statistics.Domain;
using BirdsiteLive.Twitter.Extractors;
using BirdsiteLive.Twitter.Models;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;
using Tweetinvi.Parameters;

namespace BirdsiteLive.Twitter
{
    public interface ITwitterService
    {
        TwitterUser GetUser(string username);
        ExtractedTweet GetTweet(long statusId);
        ExtractedTweet[] GetTimeline(string username, int nberTweets, long fromTweetId = -1);
    }

    public class TwitterService : ITwitterService
    {
        private readonly TwitterSettings _settings;
        private readonly ITweetExtractor _tweetExtractor;
        private readonly ITwitterStatisticsHandler _statisticsHandler;

        #region Ctor
        public TwitterService(TwitterSettings settings, ITweetExtractor tweetExtractor, ITwitterStatisticsHandler statisticsHandler)
        {
            _settings = settings;
            _tweetExtractor = tweetExtractor;
            _statisticsHandler = statisticsHandler;
            Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
        }
        #endregion

        public TwitterUser GetUser(string username)
        {
            var user = User.GetUserFromScreenName(username);
            _statisticsHandler.CalledUserApi();
            if (user == null) return null;

            return new TwitterUser
            {
                Acct = username,
                Name = user.Name,
                Description = user.Description,
                Url = $"https://twitter.com/{username}",
                ProfileImageUrl = user.ProfileImageUrlFullSize,
                ProfileBackgroundImageUrl = user.ProfileBackgroundImageUrlHttps,
                ProfileBannerURL = user.ProfileBannerURL
            };
        }

        public ExtractedTweet GetTweet(long statusId)
        {
            TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;
            var tweet = Tweet.GetTweet(statusId);
            _statisticsHandler.CalledTweetApi();
            if (tweet == null) return null; //TODO: test this
            return _tweetExtractor.Extract(tweet);
        }

        public ExtractedTweet[] GetTimeline(string username, int nberTweets, long fromTweetId = -1)
        {
            TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;

            var user = User.GetUserFromScreenName(username);
            _statisticsHandler.CalledUserApi();
            var tweets = new List<ITweet>();
            if (fromTweetId == -1)
            {
                var timeline = Timeline.GetUserTimeline(user.Id, nberTweets);
                _statisticsHandler.CalledTimelineApi();
                if (timeline != null) tweets.AddRange(timeline);
            }
            else
            {
                var timelineRequestParameters = new UserTimelineParameters
                {
                    SinceId = fromTweetId,
                    MaximumNumberOfTweetsToRetrieve = nberTweets
                };
                var timeline = Timeline.GetUserTimeline(user.Id, timelineRequestParameters);
                _statisticsHandler.CalledTimelineApi();
                if (timeline != null) tweets.AddRange(timeline);
            }

            return tweets.Select(_tweetExtractor.Extract).ToArray();
        }
    }
}
