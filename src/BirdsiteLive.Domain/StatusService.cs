using System.Collections.Generic;
using System.IO;
using System.Linq;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.Common.Settings;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace BirdsiteLive.Domain
{
    public interface IStatusService
    {
        Note GetStatus(string username, ITweet tweet);
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

        public Note GetStatus(string username, ITweet tweet)
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
                content = $"<p>{tweet.Text}</p>",
                attachment = GetAttachments(tweet.Media),
                tag = new string[0]
            };
          

            return note;
        }

        private Attachment[] GetAttachments(List<IMediaEntity> media)
        {
            var result = new List<Attachment>();

            foreach (var m in media)
            {
                var mediaUrl = GetMediaUrl(m);
                var mediaType = GetMediaType(m.MediaType, mediaUrl);
                if (mediaType == null) continue;

                var att = new Attachment
                {
                    type = "Document",
                    mediaType = mediaType,
                    url = mediaUrl
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
    }
}