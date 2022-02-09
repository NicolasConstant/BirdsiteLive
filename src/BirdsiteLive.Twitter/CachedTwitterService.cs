using System;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Twitter.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BirdsiteLive.Twitter
{
    public interface ICachedTwitterUserService : ITwitterUserService
    {
        void PurgeUser(string username);
    }

    public class CachedTwitterUserService : ICachedTwitterUserService
    {
        private readonly ITwitterUserService _twitterService;

        private readonly MemoryCache _userCache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSize(1)//Size amount
            //Priority on removing when reaching size limit (memory pressure)
            .SetPriority(CacheItemPriority.High)
            // Keep in cache for this time, reset time if accessed.
            .SetSlidingExpiration(TimeSpan.FromHours(24))
            // Remove from cache after this time, regardless of sliding expiration
            .SetAbsoluteExpiration(TimeSpan.FromDays(7));

        #region Ctor
        public CachedTwitterUserService(ITwitterUserService twitterService, InstanceSettings settings)
        {
            _twitterService = twitterService;

            _userCache = new MemoryCache(new MemoryCacheOptions()
            {
                SizeLimit = settings.UserCacheCapacity
            });
        }
        #endregion

        public TwitterUser GetUser(string username)
        {
            if (!_userCache.TryGetValue(username, out TwitterUser user))
            {
                user = _twitterService.GetUser(username);
                if(user != null) _userCache.Set(username, user, _cacheEntryOptions);
            }

            return user;
        }

        public bool IsUserApiRateLimited()
        {
            return _twitterService.IsUserApiRateLimited();
        }

        public void PurgeUser(string username)
        {
            _userCache.Remove(username);
        }
    }
}