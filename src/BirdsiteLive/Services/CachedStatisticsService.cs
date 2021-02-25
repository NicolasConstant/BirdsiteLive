using System;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Statistics.Domain;

namespace BirdsiteLive.Services
{
    public interface ICachedStatisticsService
    {
        Task<CachedStatistics> GetStatisticsAsync();
    }

    public class CachedStatisticsService : ICachedStatisticsService
    {
        private readonly ITwitterStatisticsHandler _twitterStatisticsHandler;
        private readonly ITwitterUserDal _twitterUserDal;

        private static CachedStatistics _cachedStatistics;

        #region Ctor
        public CachedStatisticsService(ITwitterStatisticsHandler twitterStatisticsHandler, ITwitterUserDal twitterUserDal)
        {
            _twitterStatisticsHandler = twitterStatisticsHandler;
            _twitterUserDal = twitterUserDal;
        }
        #endregion

        public async Task<CachedStatistics> GetStatisticsAsync()
        {
            if (_cachedStatistics == null ||
                (DateTime.UtcNow - _cachedStatistics.RefreshedTime).TotalMinutes > 15)
            {
                var twitterUserMax = _twitterStatisticsHandler.GetStatistics().UserCallsMax;
                var twitterUserCount = await _twitterUserDal.GetTwitterUsersCountAsync();
                var saturation = (int)((double)twitterUserCount / twitterUserMax * 100);

                _cachedStatistics = new CachedStatistics
                {
                    RefreshedTime = DateTime.UtcNow,
                    Saturation = saturation
                };
            }

            return _cachedStatistics;
        }
    }

    public class CachedStatistics
    {
        public DateTime RefreshedTime { get; set; }
        public int Saturation { get; set; }
    }
}