using System.Runtime.CompilerServices;

namespace BirdsiteLive.ActivityPub.Converters
{
    public class UrlFactory
    {
        public static string GetActorUrl(string domain, string username)
        {
            return $"https://{domain.ToLowerInvariant()}/users/{username.ToLowerInvariant()}";
        }

        public static string GetNoteUrl(string domain, string username, string noteId)
        {
            return $"https://{domain.ToLowerInvariant()}/users/{username.ToLowerInvariant()}/statuses/{noteId}";
        }
    }
}