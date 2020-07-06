namespace BirdsiteLive.DAL.Models
{
    public class Follower
    {
        public int Id { get; set; }
        public int FollowingAccountId { get; set; }
        
        public string Acct { get; set; }
        public string Host { get; set; }
        
        public long LastTweetSynchronizedId { get; set; }
    }
}