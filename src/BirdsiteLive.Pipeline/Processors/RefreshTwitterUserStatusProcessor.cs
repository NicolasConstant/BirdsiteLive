using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Moderation.Actions;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Twitter;
using BirdsiteLive.Twitter.Models;

namespace BirdsiteLive.Pipeline.Processors
{
    public class RefreshTwitterUserStatusProcessor : IRefreshTwitterUserStatusProcessor
    {
        private readonly ICachedTwitterUserService _twitterUserService;
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly IRemoveTwitterAccountAction _removeTwitterAccountAction;
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public RefreshTwitterUserStatusProcessor(ICachedTwitterUserService twitterUserService, ITwitterUserDal twitterUserDal, IRemoveTwitterAccountAction removeTwitterAccountAction, InstanceSettings instanceSettings)
        {
            _twitterUserService = twitterUserService;
            _twitterUserDal = twitterUserDal;
            _removeTwitterAccountAction = removeTwitterAccountAction;
            _instanceSettings = instanceSettings;
        }
        #endregion

        public async Task<UserWithDataToSync[]> ProcessAsync(SyncTwitterUser[] syncTwitterUsers, CancellationToken ct)
        {
            var usersWtData = new List<UserWithDataToSync>();

            foreach (var user in syncTwitterUsers)
            {
                TwitterUser userView = null;

                try
                {
                    userView = _twitterUserService.GetUser(user.Acct);
                }
                catch (UserNotFoundException)
                {
                    await ProcessNotFoundUserAsync(user);
                    continue;
                }
                catch (UserHasBeenSuspendedException)
                {
                    await ProcessNotFoundUserAsync(user);
                    continue;
                }
                catch (RateLimitExceededException)
                {
                    await ProcessRateLimitExceededAsync(user);
                    continue;
                }
                catch (Exception)
                {
                    // ignored
                }

                if (userView == null || userView.Protected)
                {
                    await ProcessFailingUserAsync(user);
                    continue;
                }

                user.FetchingErrorCount = 0;
                var userWtData = new UserWithDataToSync
                {
                    User = user
                };
                usersWtData.Add(userWtData);
            }
            return usersWtData.ToArray();
        }

        private async Task ProcessRateLimitExceededAsync(SyncTwitterUser user)
        {
            var dbUser = await _twitterUserDal.GetTwitterUserAsync(user.Acct);
            dbUser.LastSync = DateTime.UtcNow;
            await _twitterUserDal.UpdateTwitterUserAsync(dbUser);
        }

        private async Task ProcessNotFoundUserAsync(SyncTwitterUser user)
        {
            await _removeTwitterAccountAction.ProcessAsync(user);
        }

        private async Task ProcessFailingUserAsync(SyncTwitterUser user)
        {
            var dbUser = await _twitterUserDal.GetTwitterUserAsync(user.Acct);
            dbUser.FetchingErrorCount++;
            dbUser.LastSync = DateTime.UtcNow;

            if (dbUser.FetchingErrorCount > _instanceSettings.FailingTwitterUserCleanUpThreshold)
            {
                await _removeTwitterAccountAction.ProcessAsync(user);
            }
            else
            {
                await _twitterUserDal.UpdateTwitterUserAsync(dbUser);
            }
        }
    }
}