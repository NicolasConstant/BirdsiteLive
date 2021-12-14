using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain.BusinessUseCases;

namespace BirdsiteLive.Moderation.Actions
{
    public interface IRemoveFollowerAction
    {
        Task ProcessAsync(Follower follower);
    }

    public class RemoveFollowerAction : IRemoveFollowerAction
    {
        private readonly IRejectAllFollowingsAction _rejectAllFollowingsAction;
        private readonly IProcessDeleteUser _processDeleteUser;

        #region Ctor
        public RemoveFollowerAction(IRejectAllFollowingsAction rejectAllFollowingsAction, IProcessDeleteUser processDeleteUser)
        {
            _rejectAllFollowingsAction = rejectAllFollowingsAction;
            _processDeleteUser = processDeleteUser;
        }
        #endregion

        public async Task ProcessAsync(Follower follower)
        {
            // Perform undo following to user instance
            await _rejectAllFollowingsAction.ProcessAsync(follower);

            // Remove twitter users if no more followers
            await _processDeleteUser.ExecuteAsync(follower);
        }
    }
}