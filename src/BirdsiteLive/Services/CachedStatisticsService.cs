using System;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;

namespace BirdsiteLive.Services
{
    public interface ICachedStatisticsService
    {
        Task<CachedStatistics> GetStatisticsAsync();
    }

    public class CachedStatisticsService : ICachedStatisticsService
    {
        private readonly ITwitterUserDal _twitterUserDal;

        private static CachedStatistics _cachedStatistics;
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public CachedStatisticsService(ITwitterUserDal twitterUserDal, InstanceSettings instanceSettings)
        {
            _twitterUserDal = twitterUserDal;
            _instanceSettings = instanceSettings;
        }
        #endregion

        public async Task<CachedStatistics> GetStatisticsAsync()
        {
            if (_cachedStatistics == null ||
                (DateTime.UtcNow - _cachedStatistics.RefreshedTime).TotalMinutes > 15)
            {
                var twitterUserMax = _instanceSettings.MaxUsersCapacity;
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