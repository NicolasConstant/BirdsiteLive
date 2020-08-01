using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.Common.Settings;
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

        #region Ctor
        public StatusService(InstanceSettings instanceSettings)
        {
            _instanceSettings = instanceSettings;
        }
        #endregion

        public Note GetStatus(string username, ExtractedTweet tweet)
        {
            var actorUrl = $"https://{_instanceSettings.Domain}/users/{username}";
            var noteId = $"https://{_instanceSettings.Domain}/users/{username}/statuses/{tweet.Id}";
            var noteUrl = $"https://{_instanceSettings.Domain}/@{username}/{tweet.Id}";

            var to = $"{actorUrl}/followers";
            var apPublic = "https://www.w3.org/ns/activitystreams#Public";

            var extractedTags = ExtractTags(tweet.MessageContent);
            
            var note = new Note
            {
                id = $"{noteId}/activity",

                published = tweet.CreatedAt.ToString("s") + "Z",
                url = noteUrl,
                attributedTo = actorUrl,

                //to = new [] {to},
                //cc = new [] { apPublic },

                to = new[] { to },
                cc = new[] { apPublic },
                //cc = new string[0],

                sensitive = false,
                content = $"<p>{extractedTags.content}</p>",
                attachment = Convert(tweet.Media),
                tag = extractedTags.tags
            };
          

            return note;
        }

        private (string content, Tag[] tags) ExtractTags(string messageContent)
        {
            var regex = new Regex(@"\W(\#[a-zA-Z0-9]+\b)(?!;)");
            var match = regex.Matches(messageContent);

            var tags = new List<Tag>();
            foreach (var m in match)
            {
                var tag = m.ToString().Replace("#", string.Empty).Replace("\n", string.Empty).Trim();
                var url = $"https://{_instanceSettings.Domain}/tags/{tag}";

                tags.Add(new Tag
                {
                    name = $"#{tag}",
                    href = url,
                    type = "Hashtag"
                });

                messageContent = messageContent.Replace(
                    $"#{tag}",
                    $@"<a href=""{url}"" class=""mention hashtag"" rel=""tag"">#<span>{tag}</span></a>");
            }

            return (messageContent, new Tag[0]);
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