using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;

namespace BirdsiteLive.Moderation.Actions
{
    public interface IRemoveTwitterAccountAction
    {
        Task ProcessAsync(SyncTwitterUser twitterUser);
    }

    public class RemoveTwitterAccountAction : IRemoveTwitterAccountAction
    {
        private readonly IFollowersDal _followersDal;
        private readonly ITwitterUserDal _twitterUserDal;

        #region Ctor
        public RemoveTwitterAccountAction(IFollowersDal followersDal, ITwitterUserDal twitterUserDal)
        {
            _followersDal = followersDal;
            _twitterUserDal = twitterUserDal;
        }
        #endregion

        public async Task ProcessAsync(SyncTwitterUser twitterUser)
        {
            // Check Followers 
            var twitterUserId = twitterUser.Id;
            var followers = await _followersDal.GetFollowersAsync(twitterUserId);
            
            // Remove all Followers
            foreach (var follower in followers) 
            {
                // Perform undo following to user instance
                // TODO: Insert ActivityPub magic here

                // Remove following from DB
                if (follower.Followings.Contains(twitterUserId))
                    follower.Followings.Remove(twitterUserId);

                if (follower.FollowingsSyncStatus.ContainsKey(twitterUserId))
                    follower.FollowingsSyncStatus.Remove(twitterUserId);

                if (follower.Followings.Any())
                    await _followersDal.UpdateFollowerAsync(follower);
                else
                    await _followersDal.DeleteFollowerAsync(follower.Id);
            }

            // Remove twitter user
            await _twitterUserDal.DeleteTwitterUserAsync(twitterUser.Acct);
        }
    }
}