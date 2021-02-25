using System;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Converters;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;

namespace BirdsiteLive.Moderation.Actions
{
    public interface IRejectFollowingAction
    {
        Task ProcessAsync(Follower follower, SyncTwitterUser twitterUser);
    }

    public class RejectFollowingAction : IRejectFollowingAction
    {
        private readonly IUserService _userService;
        private readonly InstanceSettings _instanceSettings;
        
        #region Ctor
        public RejectFollowingAction(IUserService userService, InstanceSettings instanceSettings)
        {
            _userService = userService;
            _instanceSettings = instanceSettings;
        }
        #endregion

        public async Task ProcessAsync(Follower follower, SyncTwitterUser twitterUser)
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