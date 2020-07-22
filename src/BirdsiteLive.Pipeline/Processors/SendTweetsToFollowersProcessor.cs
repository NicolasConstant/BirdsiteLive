using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Domain;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Twitter;

namespace BirdsiteLive.Pipeline.Processors
{
    public class SendTweetsToFollowersProcessor : ISendTweetsToFollowersProcessor
    {
        private readonly IActivityPubService _activityPubService;
        private readonly IUserService _userService;
        private readonly IFollowersDal _followersDal;
        private readonly ITwitterUserDal _twitterUserDal;

        #region Ctor
        public SendTweetsToFollowersProcessor(IActivityPubService activityPubService, IUserService userService, IFollowersDal followersDal, ITwitterUserDal twitterUserDal)
        {
            _activityPubService = activityPubService;
            _userService = userService;
            _followersDal = followersDal;
            _twitterUserDal = twitterUserDal;
        }
        #endregion

        public async Task ProcessAsync(UserWithTweetsToSync userWithTweetsToSync, CancellationToken ct)
        {
            var user = userWithTweetsToSync.User;
            var userId = user.Id;

            foreach (var follower in userWithTweetsToSync.Followers)
            {
                var fromStatusId = follower.FollowingsSyncStatus[userId];
                var tweetsToSend = userWithTweetsToSync.Tweets.Where(x => x.Id > fromStatusId).OrderBy(x => x.Id).ToList();

                var syncStatus = fromStatusId;
                foreach (var tweet in tweetsToSend)
                {
                    var note = _userService.GetStatus(user.Acct, tweet);
                    var result = await _activityPubService.PostNewNoteActivity(note, user.Acct, tweet.Id.ToString(), follower.Host, follower.InboxUrl);
                    if (result == HttpStatusCode.Accepted)
                        syncStatus = tweet.Id;
                    else
                        break;
                }

                follower.FollowingsSyncStatus[userId] = syncStatus;
                await _followersDal.UpdateFollowerAsync(follower);
            }

            var lastPostedTweet = userWithTweetsToSync.Tweets.Select(x => x.Id).Max();
            var minimumSync = userWithTweetsToSync.Followers.Select(x => x.FollowingsSyncStatus[userId]).Min();
            await _twitterUserDal.UpdateTwitterUserAsync(userId, lastPostedTweet, minimumSync);
        }
    }
}