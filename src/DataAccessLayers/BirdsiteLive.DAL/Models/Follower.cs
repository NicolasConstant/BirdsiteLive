using System.Collections.Generic;

namespace BirdsiteLive.DAL.Models
{
    public class Follower
    {
        public int Id { get; set; }
        
        public int[] Followings { get; set; }
        public Dictionary<int, long> FollowingsSyncStatus { get; set; }

        public string Acct { get; set; }
        public string Host { get; set; }
    }
}