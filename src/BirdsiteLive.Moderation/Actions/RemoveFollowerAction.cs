using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;

namespace BirdsiteLive.Moderation.Actions
{
    public interface IRemoveFollowerAction
    {
        Task ProcessAsync(Follower follower);
    }

    public class RemoveFollowerAction : IRemoveFollowerAction
    {
        private readonly IFollowersDal _followersDal;
        private readonly ITwitterUserDal _twitterUserDal;

        #region Ctor
        public RemoveFollowerAction(IFollowersDal followersDal, ITwitterUserDal twitterUserDal)
        {
            _followersDal = followersDal;
            _twitterUserDal = twitterUserDal;
        }
        #endregion

        public async Task ProcessAsync(Follower follower)
        {
            // Perform undo following to user instance
            // TODO: Insert ActivityPub magic here

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