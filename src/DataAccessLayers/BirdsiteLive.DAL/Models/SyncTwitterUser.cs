namespace BirdsiteLive.DAL.Models
{
    public class SyncTwitterUser
    {
        public int Id { get; set; }
        public string Acct { get; set; }

        public long LastTweetPostedId { get; set; }
        public long LastTweetSynchronizedForAllFollowersId { get; set; }
    }
}