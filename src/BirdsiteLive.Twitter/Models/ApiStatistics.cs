namespace BirdsiteLive.Twitter.Models
{
    public class ApiStatistics
    {
        public int UserCallsCountMin { get; set; }
        public int UserCallsCountAvg { get; set; }
        public int UserCallsCountMax { get; set; }
        public int UserCallsMax { get; set; }
        public int TweetCallsCountMin { get; set; }
        public int TweetCallsCountAvg { get; set; }
        public int TweetCallsCountMax { get; set; }
        public int TweetCallsMax { get; set; }
        public int TimelineCallsCountMin { get; set; }
        public int TimelineCallsCountAvg { get; set; }
        public int TimelineCallsCountMax { get; set; }
        public int TimelineCallsMax { get; set; }
    }
}