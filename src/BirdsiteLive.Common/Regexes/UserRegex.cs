using System.Text.RegularExpressions;

namespace BirdsiteLive.Common.Regexes
{
    public class UserRegex
    {
        public static readonly Regex TwitterAccountRegex = new Regex(@"^[a-zA-Z0-9_]+$");
    }
}