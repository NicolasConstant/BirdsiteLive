using System;
using BirdsiteLive.Twitter.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BirdsiteLive.Twitter
{
    public class CachedTwitterService : ITwitterService
    {
        private readonly ITwitterService _twitterService;

        private MemoryCache _userCache = new MemoryCache(new MemoryCacheOptions()
        {
            SizeLimit = 5000
        });
        private MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSize(1)//Size amount
            //Priority on removing when reaching size limit (memory pressure)
            .SetPriority(CacheItemPriority.High)
            // Keep in cache for this time, reset time if accessed.
            .SetSlidingExpiration(TimeSpan.FromHours(24))
            // Remove from cache after this time, regardless of sliding expiration
            .SetAbsoluteExpiration(TimeSpan.FromDays(30));

        #region Ctor
        public CachedTwitterService(ITwitterService twitterService)
        {
            _twitterService = twitterService;
        }
        #endregion

        public TwitterUser GetUser(string username)
        {
            if (!_userCache.TryGetValue(username, out TwitterUser user))
            {
                user = _twitterService.GetUser(username);
                _userCache.Set(username, user, _cacheEntryOptions);
            }

            return user;
        }

        public ExtractedTweet GetTweet(long statusId)
        {
            return _twitterService.GetTweet(statusId);
        }

        public ExtractedTweet[] GetTimeline(string username, int nberTweets, long fromTweetId = -1)
        {
            return _twitterService.GetTimeline(username, nberTweets, fromTweetId);
        }
    }
}