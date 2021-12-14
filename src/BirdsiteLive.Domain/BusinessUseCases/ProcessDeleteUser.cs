using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;

namespace BirdsiteLive.Domain.BusinessUseCases
{
    public interface IProcessDeleteUser
    {
        Task ExecuteAsync(Follower follower);
        Task ExecuteAsync(string followerUsername, string followerDomain);
    }
    
    public class ProcessDeleteUser : IProcessDeleteUser
    {
        private readonly IFollowersDal _followersDal;
        private readonly ITwitterUserDal _twitterUserDal;

        #region Ctor
        public ProcessDeleteUser(IFollowersDal followersDal, ITwitterUserDal twitterUserDal)
        {
            _followersDal = followersDal;
            _twitterUserDal = twitterUserDal;
        }
        #endregion
        
        public async Task ExecuteAsync(string followerUsername, string followerDomain)
        {
            // Get Follower and Twitter Users
            var follower = await _followersDal.GetFollowerAsync(followerUsername, followerDomain);
            if (follower == null) return;

            await ExecuteAsync(follower);
        }

        public async Task ExecuteAsync(Follower follower)
        {
            // Remove twitter users if no more followers
            var followings = follower.Followings;
            foreach (var following in followings)
            {
                var followers = await _followersDal.GetFollowersAsync(following);
                if (followers.Length == 1 && followers.First().Id == follower.Id)
                    await _twitterUserDal.DeleteTwitterUserAsync(following);
            }

            // Remove follower from DB
            await _followersDal.DeleteFollowerAsync(follower.Id);
        }
    }
}