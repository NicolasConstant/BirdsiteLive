using System;
using System.Collections.Generic;
using System.Linq;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Statistics.Domain;
using BirdsiteLive.Twitter.Extractors;
using BirdsiteLive.Twitter.Models;
using BirdsiteLive.Twitter.Tools;
using Microsoft.Extensions.Logging;
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
        private readonly ITwitterAuthenticationInitializer _twitterAuthenticationInitializer;
        private readonly ITweetExtractor _tweetExtractor;
        private readonly ITwitterStatisticsHandler _statisticsHandler;
        private readonly ITwitterUserService _twitterUserService;
        private readonly ILogger<TwitterTweetsService> _logger;

        #region Ctor
        public TwitterTweetsService(ITwitterAuthenticationInitializer twitterAuthenticationInitializer, ITweetExtractor tweetExtractor, ITwitterStatisticsHandler statisticsHandler, ITwitterUserService twitterUserService, ILogger<TwitterTweetsService> logger)
        {
            _twitterAuthenticationInitializer = twitterAuthenticationInitializer;
            _tweetExtractor = tweetExtractor;
            _statisticsHandler = statisticsHandler;
            _twitterUserService = twitterUserService;
            _logger = logger;
        }
        #endregion

        public ExtractedTweet GetTweet(long statusId)
        {
            try
            {
                _twitterAuthenticationInitializer.EnsureAuthenticationIsInitialized();
                ExceptionHandler.SwallowWebExceptions = false;
                TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;

                var tweet = Tweet.GetTweet(statusId);
                _statisticsHandler.CalledTweetApi();
                if (tweet == null) return null; //TODO: test this
                return _tweetExtractor.Extract(tweet);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving tweet {TweetId}", statusId);
                return null;
            }
        }

        public ExtractedTweet[] GetTimeline(string username, int nberTweets, long fromTweetId = -1)
        {
            var tweets = new List<ITweet>();

            _twitterAuthenticationInitializer.EnsureAuthenticationIsInitialized();
            ExceptionHandler.SwallowWebExceptions = false;
            TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;

            var user = _twitterUserService.GetUser(username);
            if (user == null || user.Protected) return new ExtractedTweet[0];

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