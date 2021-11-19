using BirdsiteLive.Domain.Statistics;
using BirdsiteLive.Twitter.Models;

namespace BirdsiteLive.Models.StatisticsModels
{
    public class Statistics
    {
        public int FollowersCount { get; set; }
        public int FailingFollowersCount { get; set; }
        public int TwitterUserCount { get; set; }
        public int FailingTwitterUserCount { get; set; }
        public ApiStatistics TwitterStatistics { get; set; }
        public ExtractionStatistics ExtractionStatistics { get; set; }
    }
}