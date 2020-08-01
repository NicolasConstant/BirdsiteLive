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
        private readonly Regex _hastagRegex = new Regex(@"\W(\#[a-zA-Z0-9_ー]+\b)(?!;)");
        //private readonly Regex _hastagRegex = new Regex(@"(?<=[\s>]|^)#(\w*[a-zA-Z0-9_ー]+\w*)\b(?!;)");
        //private readonly Regex _hastagRegex = new Regex(@"(?<=[\s>]|^)#(\w*[a-zA-Z0-9_ー]+)\b(?!;)");
        private readonly Regex _mentionRegex = new Regex(@"\W(\@[a-zA-Z0-9_ー]+\b)(?!;)");
        //private readonly Regex _mentionRegex = new Regex(@"(?<=[\s>]|^)@(\w*[a-zA-Z0-9_ー]+\w*)\b(?!;)");
        //private readonly Regex _mentionRegex = new Regex(@"(?<=[\s>]|^)@(\w*[a-zA-Z0-9_ー]+)\b(?!;)");
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public StatusExtractor(InstanceSettings instanceSettings)
        {
            _instanceSettings = instanceSettings;
        }
        #endregion

        public (string content, Tag[] tags) ExtractTags(string messageContent)
        {
            var tags = new List<Tag>();
            messageContent = $" {messageContent} ";

            var hashtagMatch = _hastagRegex.Matches(messageContent);
            foreach (var m in hashtagMatch)
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
                    $@" <a href=""{url}"" class=""mention hashtag"" rel=""tag"">#<span>{tag}</span></a>");
            }

            var mentionMatch = _mentionRegex.Matches(messageContent);
            foreach (var m in mentionMatch)
            {
                var mention = m.ToString().Replace("@", string.Empty).Replace("\n", string.Empty).Trim();
                var url = $"https://{_instanceSettings.Domain}/users/{mention}";
                var name = $"@{mention}@{_instanceSettings.Domain}";

                tags.Add(new Tag
                {
                    name = name,
                    href = url,
                    type = "Mention"
                });

                messageContent = Regex.Replace(messageContent, m.ToString(),
                    $@" <span class=""h-card""><a href=""https://{_instanceSettings.Domain}/@{mention}"" class=""u-url mention"">@<span>{mention}</span></a></span>");
            }

            return (messageContent.Trim(), tags.ToArray());
        }
    }
}