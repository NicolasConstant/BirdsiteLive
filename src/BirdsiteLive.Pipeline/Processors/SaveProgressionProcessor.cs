using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Moderation.Actions;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Pipeline.Processors
{
    public class SaveProgressionProcessor : ISaveProgressionProcessor
    {
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly ILogger<SaveProgressionProcessor> _logger;
        private readonly IRemoveTwitterAccountAction _removeTwitterAccountAction;

        #region Ctor
        public SaveProgressionProcessor(ITwitterUserDal twitterUserDal, ILogger<SaveProgressionProcessor> logger, IRemoveTwitterAccountAction removeTwitterAccountAction)
        {
            _twitterUserDal = twitterUserDal;
            _logger = logger;
            _removeTwitterAccountAction = removeTwitterAccountAction;
        }
        #endregion

        public async Task ProcessAsync(UserWithDataToSync userWithTweetsToSync, CancellationToken ct)
        {
            try
            {
                if (userWithTweetsToSync.Tweets.Length == 0)
                {
                    _logger.LogInformation("No tweets synchronized");
                    await UpdateUserSyncDateAsync(userWithTweetsToSync.User);
                    return;
                }
                if(userWithTweetsToSync.Followers.Length == 0)
                {
                    _logger.LogInformation("No Followers found for {User}", userWithTweetsToSync.User.Acct);
                    await _removeTwitterAccountAction.ProcessAsync(userWithTweetsToSync.User);
                    return;
                }
            
                var userId = userWithTweetsToSync.User.Id;
                var followingSyncStatuses = userWithTweetsToSync.Followers.Select(x => x.FollowingsSyncStatus[userId]).ToList();
                var lastPostedTweet = userWithTweetsToSync.Tweets.Select(x => x.Id).Max();
                var minimumSync = followingSyncStatuses.Min();
                var now = DateTime.UtcNow;
                await _twitterUserDal.UpdateTwitterUserAsync(userId, lastPostedTweet, minimumSync, userWithTweetsToSync.User.FetchingErrorCount, now, userWithTweetsToSync.User.MovedTo, userWithTweetsToSync.User.MovedToAcct, userWithTweetsToSync.User.Deleted);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SaveProgressionProcessor.ProcessAsync() Exception");
                throw;
            }
        }

        private async Task UpdateUserSyncDateAsync(SyncTwitterUser user)
        {
            user.LastSync = DateTime.UtcNow;
            await _twitterUserDal.UpdateTwitterUserAsync(user);
        }
    }
}