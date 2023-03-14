using BirdsiteLive.DAL.Models;

namespace BirdsiteLive.Pipeline.Models
{
    public class TweetToDelete
    {
        public SyncTweet Tweet { get; set; }
        public bool DeleteSuccessful { get; set; }
    }
}