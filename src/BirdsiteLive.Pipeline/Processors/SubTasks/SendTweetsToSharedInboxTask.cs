using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Twitter.Models;

namespace BirdsiteLive.Pipeline.Processors.SubTasks
{
    public interface ISendTweetsToSharedInboxTask
    {
        Task ExecuteAsync(ExtractedTweet[] tweets, SyncTwitterUser user, string host, Follower[] followersPerInstance);
    }

    public class SendTweetsToSharedInboxTask : ISendTweetsToSharedInboxTask
    {
        private readonly IStatusService _statusService;
        private readonly IActivityPubService _activityPubService;
        private readonly IFollowersDal _followersDal;
        
        #region Ctor
        public SendTweetsToSharedInboxTask(IActivityPubService activityPubService, IStatusService statusService, IFollowersDal followersDal)
        {
            _activityPubService = activityPubService;
            _statusService = statusService;
            _followersDal = followersDal;
        }
        #endregion

        public async Task ExecuteAsync(ExtractedTweet[] tweets, SyncTwitterUser user, string host, Follower[] followersPerInstance)
        {
            var userId = user.Id;
            var inbox = followersPerInstance.First().SharedInboxRoute;

            var fromStatusId = followersPerInstance
                .Max(x => x.FollowingsSyncStatus[userId]);

            var tweetsToSend = tweets
                .Where(x => x.Id > fromStatusId)
                .OrderBy(x => x.Id)
                .ToList();

            var syncStatus = fromStatusId;
            try
            {
                foreach (var tweet in tweetsToSend)
                {
                    var note = _statusService.GetStatus(user.Acct, tweet);
                    var result =
                        await _activityPubService.PostNewNoteActivity(note, user.Acct, tweet.Id.ToString(), host, inbox);

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
                    foreach (var f in followersPerInstance)
                    {
                        f.FollowingsSyncStatus[userId] = syncStatus;
                        await _followersDal.UpdateFollowerAsync(f);
                    }
                }
            }
        }
    }
}