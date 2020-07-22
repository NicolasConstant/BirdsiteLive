using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Twitter;
using Tweetinvi.Models;

namespace BirdsiteLive.Pipeline.Processors
{
    public class SendTweetsToFollowersProcessor : ISendTweetsToFollowersProcessor
    {
        private readonly IActivityPubService _activityPubService;
        private readonly IStatusService _statusService;
        private readonly IFollowersDal _followersDal;

        #region Ctor
        public SendTweetsToFollowersProcessor(IActivityPubService activityPubService, IFollowersDal followersDal, IStatusService statusService)
        {
            _activityPubService = activityPubService;
            _followersDal = followersDal;
            _statusService = statusService;
        }
        #endregion

        public async Task<UserWithTweetsToSync> ProcessAsync(UserWithTweetsToSync userWithTweetsToSync, CancellationToken ct)
        {
            var user = userWithTweetsToSync.User;
            var userId = user.Id;

            foreach (var follower in userWithTweetsToSync.Followers)
            {
                try
                {
                    await ProcessFollowerAsync(userWithTweetsToSync.Tweets, follower, userId, user);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //TODO handle error
                }
            }

            return userWithTweetsToSync;
        }

        private async Task ProcessFollowerAsync(IEnumerable<ITweet> tweets, Follower follower, int userId,
            SyncTwitterUser user)
        {
            var fromStatusId = follower.FollowingsSyncStatus[userId];
            var tweetsToSend = tweets.Where(x => x.Id > fromStatusId).OrderBy(x => x.Id).ToList();

            var syncStatus = fromStatusId;
            try
            {
                foreach (var tweet in tweetsToSend)
                {
                    var note = _statusService.GetStatus(user.Acct, tweet);
                    var result = await _activityPubService.PostNewNoteActivity(note, user.Acct, tweet.Id.ToString(), follower.Host,
                        follower.InboxUrl);

                    if (result == HttpStatusCode.Accepted) 
                        syncStatus = tweet.Id;
                    else
                        throw new Exception("Posting new note activity failed");
                }
            }
            finally
            {
                if (syncStatus != fromStatusId)
                {
                    follower.FollowingsSyncStatus[userId] = syncStatus;
                    await _followersDal.UpdateFollowerAsync(follower);
                }
            }
        }
    }
}