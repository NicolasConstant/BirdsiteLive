using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;

namespace BirdsiteLive.Domain.BusinessUseCases
{
    public interface IProcessUndoFollowUser
    {
        Task ExecuteAsync(string followerUsername, string followerDomain, string twitterUsername);
    }

    public class ProcessUndoFollowUser : IProcessUndoFollowUser
    {
        private readonly IFollowersDal _followerDal;
        private readonly ITwitterUserDal _twitterUserDal;

        #region Ctor
        public ProcessUndoFollowUser(IFollowersDal followerDal, ITwitterUserDal twitterUserDal)
        {
            _followerDal = followerDal;
            _twitterUserDal = twitterUserDal;
        }
        #endregion

        public async Task ExecuteAsync(string followerUsername, string followerDomain, string twitterUsername)
        {
            // Get Follower and Twitter Users
            var follower = await _followerDal.GetFollowerAsync(followerUsername, followerDomain);
            if (follower == null) return;

            var twitterUser = await _twitterUserDal.GetTwitterUserAsync(twitterUsername);
            if (twitterUser == null) return;

            // Update Follower
            var twitterUserId = twitterUser.Id;
            if (follower.Followings.Contains(twitterUserId))
                follower.Followings.Remove(twitterUserId);

            if (follower.FollowingsSyncStatus.ContainsKey(twitterUserId))
                follower.FollowingsSyncStatus.Remove(twitterUserId);

            // Save or delete Follower
            if (follower.Followings.Any())
                await _followerDal.UpdateFollowerAsync(follower);
            else
                await _followerDal.DeleteFollowerAsync(followerUsername, followerDomain); 
            
            // Check if TwitterUser has still followers
            var followers = await _followerDal.GetFollowersAsync(twitterUser.Id);
            if (!followers.Any())
                await _twitterUserDal.DeleteTwitterUserAsync(twitterUsername);
        }
    }
}