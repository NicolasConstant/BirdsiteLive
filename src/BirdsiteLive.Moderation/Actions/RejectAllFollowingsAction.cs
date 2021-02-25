using System;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Converters;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;

namespace BirdsiteLive.Moderation.Actions
{
    public interface IRejectAllFollowingsAction
    {
        Task ProcessAsync(Follower follower);
    }

    public class RejectAllFollowingsAction : IRejectAllFollowingsAction
    {
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly IUserService _userService;
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public RejectAllFollowingsAction(ITwitterUserDal twitterUserDal, IUserService userService, InstanceSettings instanceSettings)
        {
            _twitterUserDal = twitterUserDal;
            _userService = userService;
            _instanceSettings = instanceSettings;
        }
        #endregion

        public async Task ProcessAsync(Follower follower)
        {
            foreach (var following in follower.Followings)
            {
                try
                {
                    var f = await _twitterUserDal.GetTwitterUserAsync(following);
                    var activityFollowing = new ActivityFollow
                    {
                        type = "Follow",
                        actor = follower.ActorId,
                        apObject = UrlFactory.GetActorUrl(_instanceSettings.Domain, f.Acct)
                    };

                    await _userService.SendRejectFollowAsync(activityFollowing, follower.Host);
                }
                catch (Exception) { }
            }
        }
    }
}