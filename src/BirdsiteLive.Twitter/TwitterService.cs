using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
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

        #region Ctor
        public TwitterService(TwitterSettings settings)
        {
            _settings = settings;
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
            return Extract(tweet);
        }

        private ExtractedTweet Extract(ITweet tweet)
        {
            var extractedTweet = new ExtractedTweet
            {
                Id = tweet.Id,
                InReplyToStatusId = tweet.InReplyToStatusId,
                MessageContent = ExtractMessage(tweet),
                Media = ExtractMedia(tweet.Media),
                CreatedAt = tweet.CreatedAt
            };
            return extractedTweet;
        }
        
        private string ExtractMessage(ITweet tweet)
        {
            var tweetUrls = tweet.Media.Select(x => x.URL).Distinct();
            var message = tweet.FullText;
            foreach (var tweetUrl in tweetUrls)
                message = message.Replace(tweetUrl, string.Empty).Trim();

            if (tweet.QuotedTweet != null) message = $"[Quote RT] {message}";
            if (tweet.IsRetweet)
            {
                if (tweet.RetweetedTweet != null)
                    message = $"[RT {tweet.RetweetedTweet.CreatedBy.ScreenName}] {tweet.RetweetedTweet.FullText}";
                else
                    message = message.Replace("RT", "[RT]");
            }

            return message;
        }
        
        private ExtractedMedia[] ExtractMedia(List<IMediaEntity> media)
        {
            var result = new List<ExtractedMedia>();

            foreach (var m in media)
            {
                var mediaUrl = GetMediaUrl(m);
                var mediaType = GetMediaType(m.MediaType, mediaUrl);
                if (mediaType == null) continue;

                var att = new ExtractedMedia
                {
                    MediaType = mediaType,
                    Url = mediaUrl
                };
                result.Add(att);
            }

            return result.ToArray();
        }

        private string GetMediaUrl(IMediaEntity media)
        {
            switch (media.MediaType)
            {
                case "photo": return media.MediaURLHttps;
                case "animated_gif": return media.VideoDetails.Variants[0].URL;
                case "video": return media.VideoDetails.Variants.OrderByDescending(x => x.Bitrate).First().URL;
                default: return null;
            }
        }

        private string GetMediaType(string mediaType, string mediaUrl)
        {
            switch (mediaType)
            {
                case "photo":
                    var ext = Path.GetExtension(mediaUrl);
                    switch (ext)
                    {
                        case ".jpg":
                        case ".jpeg":
                            return "image/jpeg";
                        case ".png":
                            return "image/png";
                    }
                    return null;

                case "animated_gif":
                    return "image/gif";

                case "video":
                    return "video/mp4";
            }
            return null;
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

            return tweets.Select(Extract).ToArray();
            //return tweets.Where(x => returnReplies || string.IsNullOrWhiteSpace(x.InReplyToScreenName)).ToArray();
        }
    }
}
