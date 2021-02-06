using System.Text.RegularExpressions;

namespace BirdsiteLive.Common.Regexes
{
    public class HeaderRegexes
    {
        public static readonly Regex HeaderSignature = new Regex(@"^([a-zA-Z0-9]+)=""(.+)""$");
    }
}