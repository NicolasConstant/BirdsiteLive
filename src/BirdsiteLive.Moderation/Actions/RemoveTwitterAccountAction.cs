using System;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Converters;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;

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
        private readonly IUserService _userService;
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public RemoveTwitterAccountAction(IFollowersDal followersDal, ITwitterUserDal twitterUserDal, InstanceSettings instanceSettings, IUserService userService)
        {
            _followersDal = followersDal;
            _twitterUserDal = twitterUserDal;
            _instanceSettings = instanceSettings;
            _userService = userService;
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
                await RejectFollowingAsync(follower, twitterUser);

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

        private async Task RejectFollowingAsync(Follower follower, SyncTwitterUser twitterUser)
        {
            try
            {
                var activityFollowing = new ActivityFollow
                {
                    type = "Follow",
                    actor = follower.ActorId,
                    apObject = UrlFactory.GetActorUrl(_instanceSettings.Domain, twitterUser.Acct)
                };
                await _userService.SendRejectFollowAsync(activityFollowing, follower.Host);
            }
            catch (Exception) { }
        }
    }
}