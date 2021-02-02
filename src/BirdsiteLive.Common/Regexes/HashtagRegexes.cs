using System.Text.RegularExpressions;

namespace BirdsiteLive.Common.Regexes
{
    public class HashtagRegexes
    {
        public static readonly Regex Hashtag = new Regex(@"(.)(#[a-zA-Z0-9]+)(\s|$|[.,;:!?/|-])");
    }
}