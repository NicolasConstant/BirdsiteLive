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
        private static int _previousUserCalls;
        private static int _previousTweetCalls;
        private static int _previousTimelineCalls;

        private static int _userCalls;
        private static int _tweetCalls;
        private static int _timelineCalls;

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
            _previousUserCalls = _userCalls;
            _previousTweetCalls = _tweetCalls;
            _previousTimelineCalls = _timelineCalls;

            Interlocked.Exchange(ref _userCalls, 0);
            Interlocked.Exchange(ref _tweetCalls, 0);
            Interlocked.Exchange(ref _timelineCalls, 0);
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
            return new ApiStatistics
            {
                UserCallsCount = _previousUserCalls,
                UserCallsMax = 900,
                TweetCallsCount = _previousTweetCalls,
                TweetCallsMax = 300,
                TimelineCallsCount = _previousTimelineCalls,
                TimelineCallsMax = 1500
            };
        }
    }
}