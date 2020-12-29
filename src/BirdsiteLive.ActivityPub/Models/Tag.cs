namespace BirdsiteLive.ActivityPub.Models
{
    public class Tag {
        public string type { get; set; } //Hashtag
        public string href { get; set; } //https://mastodon.social/tags/app
        public string name { get; set; } //#app
    }
}