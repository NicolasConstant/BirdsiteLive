using System.Text.RegularExpressions;

namespace BirdsiteLive.Common.Regexes
{
    public class UserRegexes
    {
        public static readonly Regex TwitterAccount = new Regex(@"^[a-zA-Z0-9_]+$");
        public static readonly Regex Mention = new Regex(@"(.?)@([a-zA-Z0-9_]+)(\s|$|[\[\]<>,;:!?/|-]|(. ))");
    }
}