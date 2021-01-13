using BirdsiteLive.Twitter.Models;

namespace BirdsiteLive.Models.StatisticsModels
{
    public class Statistics
    {
        public int FollowersCount { get; set; }
        public int TwitterUserCount { get; set; }
        public ApiStatistics TwitterStatistics { get; set; }
    }
}