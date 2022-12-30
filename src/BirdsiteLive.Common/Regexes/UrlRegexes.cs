using System.Text.RegularExpressions;

namespace BirdsiteLive.Common.Regexes
{
    public class UrlRegexes
    {
        public static readonly Regex Url = new Regex(@"(.?)(((http|ftp|https):\/\/)[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)");

        public static readonly Regex Domain = new Regex(@"^[a-zA-Z0-9\-_]+(\.[a-zA-Z0-9\-_]+)+$");
    }
}