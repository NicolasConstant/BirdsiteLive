using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.Common.Regexes;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Twitter;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Domain.Tools
{
    public interface IStatusExtractor
    {
        (string content, Tag[] tags) Extract(string messageContent, bool extractMentions = true);
    }

    public class StatusExtractor : IStatusExtractor
    {
        private readonly InstanceSettings _instanceSettings;
        private readonly ILogger<StatusExtractor> _logger;

        #region Ctor
        public StatusExtractor(InstanceSettings instanceSettings, ILogger<StatusExtractor> logger)
        {
            _instanceSettings = instanceSettings;
            _logger = logger;
        }
        #endregion

        public (string content, Tag[] tags) Extract(string messageContent, bool extractMentions = true)
        {
            var tags = new List<Tag>();

            // Replace return lines
            messageContent = Regex.Replace(messageContent, @"\r\n\r\n?|\n\n", "</p><p>");
            messageContent = Regex.Replace(messageContent, @"\r\n?|\n", "<br/>");

            //// Secure emojis
            //var emojiMatch = EmojiRegexes.Emoji.Matches(messageContent);
            //foreach (Match m in emojiMatch)
            //    messageContent = Regex.Replace(messageContent, m.ToString(), $" {m} ");

            // Extract Urls
            var urlMatch = UrlRegexes.Url.Matches(messageContent);
            foreach (Match m in urlMatch)
            {
                var url = m.Groups[2].ToString();
                var protocol = m.Groups[3].ToString();

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

                messageContent = Regex.Replace(messageContent, Regex.Escape(m.ToString()),
                    $@"{m.Groups[1]}<a href=""{url}"" rel=""nofollow noopener noreferrer"" target=""_blank""><span class=""invisible"">{protocol}</span><span class=""ellipsis"">{firstPart}</span><span class=""invisible"">{secondPart}</span></a>");
            }

            // Extract Hashtags
            var hashtagMatch = OrderByLength(HashtagRegexes.Hashtag.Matches(messageContent));
            foreach (Match m in hashtagMatch.OrderByDescending(x => x.Length))
            {
                var tag = m.Groups[2].ToString();

                if (!HashtagRegexes.HashtagName.IsMatch(tag))
                {
                    _logger.LogError("Parsing Hashtag failed: {Tag} on {Content}", tag, messageContent);
                    continue;
                }

                var url = $"https://{_instanceSettings.Domain}/tags/{tag}";

                if (tags.All(x => x.href != url))
                {
                    tags.Add(new Tag
                    {
                        name = $"#{tag}",
                        href = url,
                        type = "Hashtag"
                    });
                }

                messageContent = Regex.Replace(messageContent, Regex.Escape(m.Groups[0].ToString()),
                    $@"{m.Groups[1]}<a href=""{url}"" class=""mention hashtag"" rel=""tag"">#<span>{tag}</span></a>{m.Groups[3]}");
            }

            // Extract Mentions
            if (extractMentions)
            {
                var mentionMatch = OrderByLength(UserRegexes.Mention.Matches(messageContent));
                foreach (Match m in mentionMatch)
                {
                    var mention = m.Groups[2].ToString();

                    if (!UserRegexes.TwitterAccount.IsMatch(mention))
                    {
                        _logger.LogError("Parsing Mention failed: {Mention} on {Content}", mention, messageContent);
                        continue;
                    }

                    var url = $"https://{_instanceSettings.Domain}/users/{mention}";
                    var name = $"@{mention}@{_instanceSettings.Domain}";

                    if (tags.All(x => x.href != url))
                    {
                        tags.Add(new Tag
                        {
                            name = name,
                            href = url,
                            type = "Mention"
                        });
                    }

                    messageContent = Regex.Replace(messageContent, Regex.Escape(m.Groups[0].ToString()),
                        $@"{m.Groups[1]}<span class=""h-card""><a href=""https://{_instanceSettings.Domain}/@{mention}"" class=""u-url mention"">@<span>{mention}</span></a></span>{m.Groups[3]}");
                }
            }

            return (messageContent.Trim(), tags.ToArray());
        }
        
        private IEnumerable<Match> OrderByLength(MatchCollection matches)
        {
            var result = new List<Match>();
            foreach (Match m in matches) result.Add(m);

            result = result
                .OrderBy(x => x.Length)
                .GroupBy(p => p.Value)
                .Select(g => g.First())
                .ToList();

            return result;
        }
    }
}