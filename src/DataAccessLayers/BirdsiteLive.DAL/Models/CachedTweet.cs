using System;

namespace BirdsiteLive.DAL.Models
{
    public class CachedTweet
    {
        public int UserId { get; set; }

        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Text { get; set; }
        public string FullText { get; set; }

        public long? InReplyToStatusId { get; set; }
        public string InReplyToStatusIdStr { get; set; }
        public long? InReplyToUserId { get; set; }
        public string InReplyToUserIdStr { get; set; }
        public string InReplyToScreenName { get; set; }

        // List<IHashtagEntity> Hashtags { get; }
        //List<IUrlEntity> Urls { get; }
        //List<IMediaEntity> Media { get; }
        //List<IUserMentionEntity> UserMentions { get; }
        //List<ITweet> Retweets { get; set; }
        public bool IsRetweet { get; }
        public CachedTweet RetweetedTweet { get; }
        public string Url { get; }
    }

}