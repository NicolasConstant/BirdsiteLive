using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Converters;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain.Statistics;
using BirdsiteLive.Domain.Tools;
using BirdsiteLive.Twitter.Models;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace BirdsiteLive.Domain
{
    public interface IStatusService
    {
        Note GetStatus(string username, ExtractedTweet tweet);
    }

    public class StatusService : IStatusService
    {
        private readonly InstanceSettings _instanceSettings;
        private readonly IStatusExtractor _statusExtractor;
        private readonly IExtractionStatisticsHandler _statisticsHandler;
        
        #region Ctor
        public StatusService(InstanceSettings instanceSettings, IStatusExtractor statusExtractor, IExtractionStatisticsHandler statisticsHandler)
        {
            _instanceSettings = instanceSettings;
            _statusExtractor = statusExtractor;
            _statisticsHandler = statisticsHandler;
        }
        #endregion

        public Note GetStatus(string username, ExtractedTweet tweet)
        {
            var actorUrl = UrlFactory.GetActorUrl(_instanceSettings.Domain, username);
            var noteUrl = UrlFactory.GetNoteUrl(_instanceSettings.Domain, username, tweet.Id.ToString());

            var to = $"{actorUrl}/followers";
            var apPublic = "https://www.w3.org/ns/activitystreams#Public";

            var extractedTags = _statusExtractor.Extract(tweet.MessageContent);
            _statisticsHandler.ExtractedStatus(extractedTags.tags.Count(x => x.type == "Mention"));

            // Replace RT by a link
            var content = extractedTags.content;
            if (content.Contains("{RT}") && tweet.IsRetweet)
            {
                if (!string.IsNullOrWhiteSpace(tweet.RetweetUrl))
                    content = content.Replace("{RT}",
                        $@"<a href=""{tweet.RetweetUrl}"" rel=""nofollow noopener noreferrer"" target=""_blank"">RT</a>");
                else
                    content = content.Replace("{RT}", "RT");
            }

            string inReplyTo = null;
            if (tweet.InReplyToStatusId != default)
                inReplyTo = $"https://{_instanceSettings.Domain}/users/{tweet.InReplyToAccount.ToLowerInvariant()}/statuses/{tweet.InReplyToStatusId}";

            var note = new Note
            {
                id = noteUrl,

                published = tweet.CreatedAt.ToString("s") + "Z",
                url = noteUrl,
                attributedTo = actorUrl,

                inReplyTo = inReplyTo,
                //to = new [] {to},
                //cc = new [] { apPublic },

                to = new[] { to },
                //cc = new[] { apPublic },
                cc = new string[0],

                sensitive = false,
                content = $"<p>{content}</p>",
                attachment = Convert(tweet.Media),
                tag = extractedTags.tags
            };

            return note;
        }

        private Attachment[] Convert(ExtractedMedia[] media)
        {
            if(media == null) return new Attachment[0];
            return media.Select(x =>
            {
                return new Attachment
                {
                    type = "Document",
                    url = x.Url,
                    mediaType = x.MediaType
                };
            }).ToArray();
        }
    }
}