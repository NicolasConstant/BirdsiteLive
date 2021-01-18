using System.Collections.Generic;
using System.Linq;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Statistics.Domain;
using BirdsiteLive.Twitter.Extractors;
using BirdsiteLive.Twitter.Models;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace BirdsiteLive.Twitter
{
    public interface ITwitterTweetsService
    {
        ExtractedTweet GetTweet(long statusId);
        ExtractedTweet[] GetTimeline(string username, int nberTweets, long fromTweetId = -1);
    }

    public class TwitterTweetsService : ITwitterTweetsService
    {
        private readonly TwitterSettings _settings;
        private readonly ITweetExtractor _tweetExtractor;
        private readonly ITwitterStatisticsHandler _statisticsHandler;
        private readonly ITwitterUserService _twitterUserService;

        #region Ctor
        public TwitterTweetsService(TwitterSettings settings, ITweetExtractor tweetExtractor, ITwitterStatisticsHandler statisticsHandler, ITwitterUserService twitterUserService)
        {
            _settings = settings;
            _tweetExtractor = tweetExtractor;
            _statisticsHandler = statisticsHandler;
            _twitterUserService = twitterUserService;
            Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
        }
        #endregion
        
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
            
            var user = _twitterUserService.GetUser(username);

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