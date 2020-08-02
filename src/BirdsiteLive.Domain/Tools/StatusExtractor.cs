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
        private readonly Regex _urlRegex = new Regex(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)");
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

            // Replace return lines
            messageContent = Regex.Replace(messageContent, @"\r\n\r\n?|\n\n", "</p><p>");
            messageContent = Regex.Replace(messageContent, @"\r\n?|\n", "<br/>");

            // Extract Urls
            var urlMatch = _urlRegex.Matches(messageContent);
            foreach (var m in urlMatch)
            {
                var url = m.ToString().Replace("\n", string.Empty).Trim();

                var protocol = "https://";
                if (url.StartsWith("http://")) protocol = "http://";
                else if (url.StartsWith("ftp://")) protocol = "ftp://";

                var truncatedUrl = url.Replace(protocol, string.Empty);

                if (truncatedUrl.StartsWith("www."))
                {
                    protocol += "www.";
                    truncatedUrl = truncatedUrl.Replace("www.", string.Empty);
                }

                var firstPart = truncatedUrl;
                var secondPart = string.Empty;

                if (truncatedUrl.Length > 30)
                {
                    firstPart = truncatedUrl.Substring(0, 30);
                    secondPart = truncatedUrl.Substring(30);
                }

                messageContent = Regex.Replace(messageContent, m.ToString(),
                    $@" <a href=""{url}"" rel=""nofollow noopener noreferrer"" target=""_blank""><span class=""invisible"">{protocol}</span><span class=""ellipsis"">{firstPart}</span><span class=""invisible"">{secondPart}</span></a>");
            }

            // Extract Hashtags
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

            // Extract Mentions
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