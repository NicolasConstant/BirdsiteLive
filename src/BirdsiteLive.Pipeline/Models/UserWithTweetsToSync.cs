using BirdsiteLive.DAL.Models;
using Tweetinvi.Models;

namespace BirdsiteLive.Pipeline.Models
{
    public class UserWithTweetsToSync
    {
        public SyncTwitterUser User { get; set; }
        public ITweet[] Tweets { get; set; }
        public Follower[] Followers { get; set; }
    }
}