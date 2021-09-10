using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Pipeline.Processors
{
    public class SaveProgressionProcessor : ISaveProgressionProcessor
    {
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly ILogger<SaveProgressionProcessor> _logger;

        #region Ctor
        public SaveProgressionProcessor(ITwitterUserDal twitterUserDal, ILogger<SaveProgressionProcessor> logger)
        {
            _twitterUserDal = twitterUserDal;
            _logger = logger;
        }
        #endregion

        public async Task ProcessAsync(UserWithDataToSync userWithTweetsToSync, CancellationToken ct)
        {
            try
            {
                if (userWithTweetsToSync.Tweets.Length == 0)
                {
                    _logger.LogWarning("No tweets synchronized");
                    return;
                }
                if(userWithTweetsToSync.Followers.Length == 0)
                {
                    _logger.LogWarning("No Followers found for {User}", userWithTweetsToSync.User.Acct);
                    return;
                }
            
                var userId = userWithTweetsToSync.User.Id;
                var followingSyncStatuses = userWithTweetsToSync.Followers.Select(x => x.FollowingsSyncStatus[userId]).ToList();
            
                if (followingSyncStatuses.Count == 0)
                {
                    _logger.LogWarning("No Followers sync found for {User}, Id: {UserId}", userWithTweetsToSync.User.Acct, userId);
                    return;
                }

                var lastPostedTweet = userWithTweetsToSync.Tweets.Select(x => x.Id).Max();
                var minimumSync = followingSyncStatuses.Min();
                var now = DateTime.UtcNow;
                await _twitterUserDal.UpdateTwitterUserAsync(userId, lastPostedTweet, minimumSync, userWithTweetsToSync.User.FetchingErrorCount, now);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SaveProgressionProcessor.ProcessAsync() Exception");
                throw;
            }
        }
    }
}