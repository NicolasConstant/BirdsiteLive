using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BirdsiteLive.ActivityPub;
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
                content = $"<p>{tweet.MessageContent}</p>",
                attachment = Convert(tweet.Media),
                tag = new string[0]
            };
          

            return note;
        }

        private Attachment[] Convert(ExtractedMedia[] media)
        {
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