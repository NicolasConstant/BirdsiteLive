namespace BirdsiteLive.DAL.Models
{
    public class CachedTweet
    {
        public long Id { get; set; }
        public long TwitterUserId { get; set; }
        
        public string TweetData { get; set; } 
    }
}