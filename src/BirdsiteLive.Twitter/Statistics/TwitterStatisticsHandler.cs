using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Timers;
using BirdsiteLive.Twitter.Models;

namespace BirdsiteLive.Statistics.Domain
{
    public interface ITwitterStatisticsHandler
    {
        void CalledUserApi();
        void CalledTweetApi();
        void CalledTimelineApi();
        ApiStatistics GetStatistics();
    }

    //Rate limits: https://developer.twitter.com/en/docs/twitter-api/v1/rate-limits
    public class TwitterStatisticsHandler : ITwitterStatisticsHandler
    {
        private static int _userCalls;
        private static int _tweetCalls;
        private static int _timelineCalls;

        private static ConcurrentDictionary<DateTime, ApiStatisticsSnapshot> _snapshots = new ConcurrentDictionary<DateTime, ApiStatisticsSnapshot>();

        private static System.Timers.Timer _resetTimer;

        #region Ctor
        public TwitterStatisticsHandler()
        {
            if (_resetTimer == null)
            {
                _resetTimer = new System.Timers.Timer();
                _resetTimer.Elapsed += OnTimeResetEvent;
                _resetTimer.Interval = 15 * 60 * 1000; // 15"
                _resetTimer.Enabled = true;
            }
        }
        #endregion

        private void OnTimeResetEvent(object sender, ElapsedEventArgs e)
        {
            // Add snapshot
            var snapshot = new ApiStatisticsSnapshot(_userCalls, _tweetCalls, _timelineCalls);
            bool success;
            do
            {
                success = _snapshots.TryAdd(snapshot.SnapshotDate, snapshot);
            } while (!success);
            
            // Reset
            Interlocked.Exchange(ref _userCalls, 0);
            Interlocked.Exchange(ref _tweetCalls, 0);
            Interlocked.Exchange(ref _timelineCalls, 0);

            // Clean up 
            var now = DateTime.UtcNow;
            var oldSnapshots = _snapshots.Keys.Where(x => (now - x).TotalHours > 24).ToList();
            foreach (var old in oldSnapshots) _snapshots.TryRemove(old, out var data);
        }

        public void CalledUserApi()  //GET users/show - 900/15mins
        {
            Interlocked.Increment(ref _userCalls);
        }

        public void CalledTweetApi()  //GET statuses/lookup - 300/15mins
        {
            Interlocked.Increment(ref _tweetCalls);
        }

        public void CalledTimelineApi()  // GET statuses/user_timeline - 1500/15 mins
        {
            Interlocked.Increment(ref _timelineCalls);
        }

        public ApiStatistics GetStatistics()
        {
            var snapshots = _snapshots.Values.ToList();
            var userCalls = snapshots.Select(x => x.UserCalls).ToList();
            var tweetCalls = snapshots.Select(x => x.TweetCalls).ToList();
            var timelineCalls = snapshots.Select(x => x.TimelineCalls).ToList();

            return new ApiStatistics
            {
                UserCallsCountMin = userCalls.Any() ? userCalls.Min() : 0,
                UserCallsCountAvg = userCalls.Any() ? (int)userCalls.Average() : 0,
                UserCallsCountMax = userCalls.Any() ? userCalls.Max() : 0,
                UserCallsMax = 300,
                TweetCallsCountMin = tweetCalls.Any() ? tweetCalls.Min() : 0,
                TweetCallsCountAvg = tweetCalls.Any() ? (int)tweetCalls.Average() : 0,
                TweetCallsCountMax = tweetCalls.Any() ? tweetCalls.Max() : 0,
                TweetCallsMax = 300,
                TimelineCallsCountMin = timelineCalls.Any() ? timelineCalls.Min() : 0,
                TimelineCallsCountAvg = timelineCalls.Any() ? (int)timelineCalls.Average() : 0,
                TimelineCallsCountMax = timelineCalls.Any() ? timelineCalls.Max() : 0,
                TimelineCallsMax = 1000
            };
        }
    }

    internal class ApiStatisticsSnapshot 
    {
        #region Ctor
        public ApiStatisticsSnapshot(int userCalls, int tweetCalls, int timelineCalls)
        {
            UserCalls = userCalls;
            TweetCalls = tweetCalls;
            TimelineCalls = timelineCalls;
            SnapshotDate = DateTime.UtcNow;
        }
        #endregion

        public DateTime SnapshotDate { get;  }
        public int UserCalls { get; set; }
        public int TweetCalls { get; set; }
        public int TimelineCalls { get; set; }
    }
}