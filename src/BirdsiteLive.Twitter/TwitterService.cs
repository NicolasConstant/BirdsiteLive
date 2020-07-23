using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
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

        #region Ctor
        public TwitterService(TwitterSettings settings, ITweetExtractor tweetExtractor)
        {
            _settings = settings;
            _tweetExtractor = tweetExtractor;
            Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
        }
        #endregion

        public TwitterUser GetUser(string username)
        {
            var user = User.GetUserFromScreenName(username);
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
            return _tweetExtractor.Extract(tweet);
        }

       

        public ExtractedTweet[] GetTimeline(string username, int nberTweets, long fromTweetId = -1)
        {
            TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;

            var user = User.GetUserFromScreenName(username);
            var tweets = new List<ITweet>();
            if (fromTweetId == -1)
            {
                var timeline = Timeline.GetUserTimeline(user.Id, nberTweets);
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
                if (timeline != null) tweets.AddRange(timeline);
            }

            return tweets.Select(_tweetExtractor.Extract).ToArray();
            //return tweets.Where(x => returnReplies || string.IsNullOrWhiteSpace(x.InReplyToScreenName)).ToArray();
        }
    }
}
