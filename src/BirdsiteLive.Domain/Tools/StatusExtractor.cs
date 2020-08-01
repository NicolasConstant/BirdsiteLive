using System.Collections.Generic;
using System.Text.RegularExpressions;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.Common.Settings;

namespace BirdsiteLive.Domain.Tools
{
    public interface IStatusExtractor
    {
        (string content, Tag[] tags) ExtractTags(string messageContent);
    }

    public class StatusExtractor : IStatusExtractor
    {
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public StatusExtractor(InstanceSettings instanceSettings)
        {
            _instanceSettings = instanceSettings;
        }
        #endregion

        public (string content, Tag[] tags) ExtractTags(string messageContent)
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

                messageContent = Regex.Replace(messageContent, m.ToString(),
                    $@"<a href=""{url}"" class=""mention hashtag"" rel=""tag"">#<span>{tag}</span></a>");
                
                //messageContent = messageContent.Replace(
                //    $"#{tag}",
                //    $@"<a href=""{url}"" class=""mention hashtag"" rel=""tag"">#<span>{tag}</span></a>");
            }

            return (messageContent, new Tag[0]);
        }
    }
}