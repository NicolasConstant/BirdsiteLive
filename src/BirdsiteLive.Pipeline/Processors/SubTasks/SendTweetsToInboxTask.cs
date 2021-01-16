using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Twitter.Models;

namespace BirdsiteLive.Pipeline.Processors.SubTasks
{
    public interface ISendTweetsToInboxTask
    {
        Task ExecuteAsync(IEnumerable<ExtractedTweet> tweets, Follower follower, SyncTwitterUser user);
    }

    public class SendTweetsToInboxTask : ISendTweetsToInboxTask
    {
        private readonly IActivityPubService _activityPubService;
        private readonly IStatusService _statusService;
        private readonly IFollowersDal _followersDal;

        #region Ctor
        public SendTweetsToInboxTask(IActivityPubService activityPubService, IStatusService statusService, IFollowersDal followersDal)
        {
            _activityPubService = activityPubService;
            _statusService = statusService;
            _followersDal = followersDal;
        }
        #endregion

        public async Task ExecuteAsync(IEnumerable<ExtractedTweet> tweets, Follower follower, SyncTwitterUser user)
        {
            var userId = user.Id;
            var fromStatusId = follower.FollowingsSyncStatus[userId];
            var tweetsToSend = tweets
                .Where(x => x.Id > fromStatusId)
                .OrderBy(x => x.Id)
                .ToList();

            var inbox = follower.InboxRoute;

            var syncStatus = fromStatusId;
            try
            {
                foreach (var tweet in tweetsToSend)
                {
                    var note = _statusService.GetStatus(user.Acct, tweet);
                    await _activityPubService.PostNewNoteActivity(note, user.Acct, tweet.Id.ToString(), follower.Host, inbox);
                    syncStatus = tweet.Id;
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