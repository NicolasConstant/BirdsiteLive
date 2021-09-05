using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Moderation.Actions;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Twitter;

namespace BirdsiteLive.Pipeline.Processors
{
    public class RefreshTwitterUserStatusProcessor : IRefreshTwitterUserStatusProcessor
    {
        private const int FetchingErrorCountThreshold = 10;
        private readonly ICachedTwitterUserService _twitterUserService;
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly IRemoveTwitterAccountAction _removeTwitterAccountAction;

        #region Ctor
        public RefreshTwitterUserStatusProcessor(ICachedTwitterUserService twitterUserService)
        {
            _twitterUserService = twitterUserService;
        }
        #endregion

        public async Task<UserWithDataToSync[]> ProcessAsync(SyncTwitterUser[] syncTwitterUsers, CancellationToken ct)
        {
            var usersWtData = new List<UserWithDataToSync>();

            foreach (var user in syncTwitterUsers)
            {
                var userView = _twitterUserService.GetUser(user.Acct);
                if (userView == null)
                {
                    await AnalyseFailingUserAsync(user);
                }
                else if (!userView.Protected)
                {
                    var userWtData = new UserWithDataToSync
                    {
                        User = user
                    };
                    usersWtData.Add(userWtData);
                }
            }

            return usersWtData.ToArray();
        }

        private async Task AnalyseFailingUserAsync(SyncTwitterUser user)
        {
            var dbUser = await _twitterUserDal.GetTwitterUserAsync(user.Acct);
            dbUser.FetchingErrorCount++;

            if (dbUser.FetchingErrorCount > FetchingErrorCountThreshold)
            {
                await _removeTwitterAccountAction.ProcessAsync(user);
            }
            else
            {
                await _twitterUserDal.UpdateTwitterUserAsync(dbUser);
            }

            // Purge
            _twitterUserService.PurgeUser(user.Acct);
        }
    }
}