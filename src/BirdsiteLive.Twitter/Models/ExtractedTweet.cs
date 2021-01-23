using System;
using System.Net.Sockets;

namespace BirdsiteLive.Twitter.Models
{
    public class ExtractedTweet
    {
        public long Id { get; set; }
        public long? InReplyToStatusId { get; set; }
        public string MessageContent { get; set; }
        public ExtractedMedia[] Media { get; set; }
        public DateTime CreatedAt { get; set; }
        public string InReplyToAccount { get; set; }
        public bool IsReply { get; set; }
        public bool IsThread { get; set; }
    }
}