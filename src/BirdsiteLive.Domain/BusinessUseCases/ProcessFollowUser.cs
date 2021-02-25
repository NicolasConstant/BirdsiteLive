using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;

namespace BirdsiteLive.Domain.BusinessUseCases
{
    public interface IProcessFollowUser
    {
        Task ExecuteAsync(string followerUsername, string followerDomain, string twitterUsername, string followerInbox, string sharedInbox, string followerActorId);
    }

    public class ProcessFollowUser : IProcessFollowUser
    {
        private readonly IFollowersDal _followerDal;
        private readonly ITwitterUserDal _twitterUserDal;

        #region Ctor
        public ProcessFollowUser(IFollowersDal followerDal, ITwitterUserDal twitterUserDal)
        {
            _followerDal = followerDal;
            _twitterUserDal = twitterUserDal;
        }
        #endregion

        public async Task ExecuteAsync(string followerUsername, string followerDomain, string twitterUsername, string followerInbox, string sharedInbox, string followerActorId)
        {
            // Get Follower and Twitter Users
            var follower = await _followerDal.GetFollowerAsync(followerUsername, followerDomain);
            if (follower == null)
            {
                await _followerDal.CreateFollowerAsync(followerUsername, followerDomain, followerInbox, sharedInbox, followerActorId);
                follower = await _followerDal.GetFollowerAsync(followerUsername, followerDomain);
            }

            var twitterUser = await _twitterUserDal.GetTwitterUserAsync(twitterUsername);
            if (twitterUser == null)
            {
                await _twitterUserDal.CreateTwitterUserAsync(twitterUsername, -1);
                twitterUser = await _twitterUserDal.GetTwitterUserAsync(twitterUsername);
            }

            // Update Follower
            var twitterUserId = twitterUser.Id;
            if(!follower.Followings.Contains(twitterUserId))
                follower.Followings.Add(twitterUserId);

            if(!follower.FollowingsSyncStatus.ContainsKey(twitterUserId))
                follower.FollowingsSyncStatus.Add(twitterUserId, -1);
            
            // Save Follower
            await _followerDal.UpdateFollowerAsync(follower);
        }
    }
}