using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BirdsiteLive.Twitter.Models;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace BirdsiteLive.Twitter.Extractors
{
    public interface ITweetExtractor
    {
        ExtractedTweet Extract(ITweet tweet);
    }

    public class TweetExtractor : ITweetExtractor
    {
        public ExtractedTweet Extract(ITweet tweet)
        {
            var extractedTweet = new ExtractedTweet
            {
                Id = tweet.Id,
                InReplyToStatusId = tweet.InReplyToStatusId,
                InReplyToAccount = tweet.InReplyToScreenName,
                MessageContent = ExtractMessage(tweet),
                Media = ExtractMedia(tweet.Media),
                CreatedAt = tweet.CreatedAt.ToUniversalTime(),
                IsReply = tweet.InReplyToUserId != null,
                IsThread = tweet.InReplyToUserId != null && tweet.InReplyToUserId == tweet.CreatedBy.Id,
                IsRetweet = tweet.IsRetweet || tweet.QuotedStatusId != null,
                RetweetUrl = ExtractRetweetUrl(tweet)
            };

            return extractedTweet;
        }

        private string ExtractRetweetUrl(ITweet tweet)
        {
            if (tweet.IsRetweet && tweet.FullText.Contains("https://t.co/"))
            {
                var retweetId = tweet.FullText.Split(new[] { "https://t.co/" }, StringSplitOptions.RemoveEmptyEntries).Last();
                return $"https://t.co/{retweetId}";
            }

            return null;
        }

        public string ExtractMessage(ITweet tweet)
        {
            var tweetUrls = tweet.Media.Select(x => x.URL).Distinct();
            var message = tweet.FullText;
            foreach (var tweetUrl in tweetUrls)
            {
                if(tweet.IsRetweet)
                    message = tweet.RetweetedTweet.FullText.Replace(tweetUrl, string.Empty).Trim();
                else 
                    message = message.Replace(tweetUrl, string.Empty).Trim();
            }

            if (tweet.QuotedTweet != null) message = $"[Quote {{RT}}]{Environment.NewLine}{message}";
            if (tweet.IsRetweet)
            {
                if (tweet.RetweetedTweet != null)
                    message = $"[{{RT}} @{tweet.RetweetedTweet.CreatedBy.ScreenName}]{Environment.NewLine}{message}";
                else
                    message = message.Replace("RT", "[{{RT}}]");
            }

            // Expand URLs
            foreach (var url in tweet.Urls.OrderByDescending(x => x.URL.Length))
                message = message.Replace(url.URL, url.ExpandedURL);

            return message;
        }

        public ExtractedMedia[] ExtractMedia(List<IMediaEntity> media)
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

        public string GetMediaUrl(IMediaEntity media)
        {
            switch (media.MediaType)
            {
                case "photo": return media.MediaURLHttps;
                case "animated_gif": return media.VideoDetails.Variants[0].URL;
                case "video": return media.VideoDetails.Variants.OrderByDescending(x => x.Bitrate).First().URL;
                default: return null;
            }
        }

        public string GetMediaType(string mediaType, string mediaUrl)
        {
            switch (mediaType)
            {
                case "photo":
                    var pExt = Path.GetExtension(mediaUrl);
                    switch (pExt)
                    {
                        case ".jpg":
                        case ".jpeg":
                            return "image/jpeg";
                        case ".png":
                            return "image/png";
                    }
                    return null;

                case "animated_gif":
                    var vExt = Path.GetExtension(mediaUrl);
                    switch (vExt)
                    {
                        case ".gif":
                            return "image/gif";
                        case ".mp4":
                            return "video/mp4";
                    }
                    return "image/gif";
                case "video":
                    return "video/mp4";
            }
            return null;
        }
    }
}