using System.Collections.Generic;

namespace BirdsiteLive.DAL.Models
{
    public class Follower
    {
        public int Id { get; set; }
        
        public List<int> Followings { get; set; }
        public Dictionary<int, long> FollowingsSyncStatus { get; set; }

        public string ActorId { get; set; }
        public string Acct { get; set; }
        public string Host { get; set; }
        public string InboxRoute { get; set; }
        public string SharedInboxRoute { get; set; }

        public int PostingErrorCount { get; set; }
    }
}