using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;

namespace BirdsiteLive.Pipeline.Tools
{
    public interface IMaxUsersNumberProvider
    {
        Task<int> GetMaxUsersNumberAsync();
    }

    public class MaxUsersNumberProvider : IMaxUsersNumberProvider
    {
        private readonly InstanceSettings _instanceSettings;
        private readonly ITwitterUserDal _twitterUserDal;
        
        private int _totalUsersCount = -1;
        private int _warmUpIterations;
        private const int WarmUpMaxCapacity = 200;

        #region Ctor
        public MaxUsersNumberProvider(InstanceSettings instanceSettings, ITwitterUserDal twitterUserDal)
        {
            _instanceSettings = instanceSettings;
            _twitterUserDal = twitterUserDal;
        }
        #endregion

        public async Task<int> GetMaxUsersNumberAsync()
        {
            // Init data
            if (_totalUsersCount == -1) 
            {
                _totalUsersCount = await _twitterUserDal.GetTwitterUsersCountAsync();
                _warmUpIterations = (int)(_totalUsersCount / (float)WarmUpMaxCapacity);
            }

            // Return if warm up ended
            if (_warmUpIterations <= 0) return _instanceSettings.MaxUsersCapacity;

            // Calculate warm up value
            var maxUsers = _warmUpIterations > 0
                ? WarmUpMaxCapacity
                : _instanceSettings.MaxUsersCapacity;
            _warmUpIterations--;
            return maxUsers;
        }
    }
}