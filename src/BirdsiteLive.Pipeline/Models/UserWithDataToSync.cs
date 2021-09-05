using BirdsiteLive.DAL.Models;
using BirdsiteLive.Twitter.Models;
using Tweetinvi.Models;

namespace BirdsiteLive.Pipeline.Models
{
    public class UserWithDataToSync
    {
        public SyncTwitterUser User { get; set; }
        public ExtractedTweet[] Tweets { get; set; }
        public Follower[] Followers { get; set; }
    }
}