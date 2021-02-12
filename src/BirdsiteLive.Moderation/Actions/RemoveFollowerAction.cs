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
    public interface IRemoveFollowerAction
    {
        Task ProcessAsync(Follower follower);
    }

    public class RemoveFollowerAction : IRemoveFollowerAction
    {
        private readonly IFollowersDal _followersDal;
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly IUserService _userService;
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public RemoveFollowerAction(IFollowersDal followersDal, ITwitterUserDal twitterUserDal, IUserService userService, InstanceSettings instanceSettings)
        {
            _followersDal = followersDal;
            _twitterUserDal = twitterUserDal;
            _userService = userService;
            _instanceSettings = instanceSettings;
        }
        #endregion

        public async Task ProcessAsync(Follower follower)
        {
            // Perform undo following to user instance
            await RejectAllFollowingsAsync(follower);

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

        private async Task RejectAllFollowingsAsync(Follower follower)
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