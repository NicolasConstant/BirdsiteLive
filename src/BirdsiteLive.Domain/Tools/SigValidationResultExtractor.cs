using System.Linq;

namespace BirdsiteLive.Domain.Tools
{
    public class SigValidationResultExtractor
    {
        public static string GetUserName(SignatureValidationResult result)
        {
            return result.User.preferredUsername.ToLowerInvariant().Trim();
        }

        public static string GetHost(SignatureValidationResult result)
        {
            return result.User.url.Replace("https://", string.Empty).Split('/').First();
        }

        public static string GetSharedInbox(SignatureValidationResult result)
        {
            return result.User?.endpoints?.sharedInbox;
        }
    }
}