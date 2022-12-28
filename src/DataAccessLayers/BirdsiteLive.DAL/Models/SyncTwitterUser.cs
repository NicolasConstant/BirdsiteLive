using System;

namespace BirdsiteLive.DAL.Models
{
    public class SyncTwitterUser
    {
        public int Id { get; set; }
        public string Acct { get; set; }

        public long LastTweetPostedId { get; set; }
        public long LastTweetSynchronizedForAllFollowersId { get; set; }

        public DateTime LastSync { get; set; }

        public int FetchingErrorCount { get; set; }

        public string MovedTo { get; set; }
        public string MovedToAcct { get; set; }

        public bool Deleted { get; set; }
    }
}