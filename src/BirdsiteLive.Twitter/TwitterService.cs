using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Twitter.Models;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace BirdsiteLive.Twitter
{
    public interface ITwitterService
    {
        TwitterUser GetUser(string username);
        ITweet GetTweet(long statusId);
        ITweet[] GetTimeline(string username, int nberTweets, long fromTweetId = -1);
    }

    public class TwitterService : ITwitterService
    {
        private readonly TwitterSettings _settings;

        #region Ctor
        public TwitterService(TwitterSettings settings)
        {
            _settings = settings;
            Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
        }
        #endregion

        public TwitterUser GetUser(string username)
        {
            //Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
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

        public ITweet GetTweet(long statusId)
        {
            //Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
            var tweet = Tweet.GetTweet(statusId);
            return tweet;
        }

        public ITweet[] GetTimeline(string username, int nberTweets, long fromTweetId = -1)
        {
            //Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
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

            return tweets.ToArray();
            //return tweets.Where(x => returnReplies || string.IsNullOrWhiteSpace(x.InReplyToScreenName)).ToArray();
        }
    }
}
