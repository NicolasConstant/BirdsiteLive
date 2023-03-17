using System;

namespace BirdsiteLive.DAL.Models
{
    public class SyncTweet
    {
        public long Id { get; set; }
        
        public string Acct { get; set; }
        public string Host { get; set; } //TODO
        public string Inbox { get; set; }
        public long TweetId { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}