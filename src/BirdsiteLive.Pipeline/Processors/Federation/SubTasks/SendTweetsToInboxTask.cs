using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Twitter.Models;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Pipeline.Processors.SubTasks
{
    public interface ISendTweetsToInboxTask
    {
        Task ExecuteAsync(IEnumerable<ExtractedTweet> tweets, Follower follower, SyncTwitterUser user);
    }

    public class SendTweetsToInboxTask : SendTweetsTaskBase, ISendTweetsToInboxTask
    {
        private readonly IActivityPubService _activityPubService;
        private readonly IStatusService _statusService;
        private readonly IFollowersDal _followersDal;
        private readonly InstanceSettings _settings;
        private readonly ILogger<SendTweetsToInboxTask> _logger;
        
        #region Ctor
        public SendTweetsToInboxTask(IActivityPubService activityPubService, IStatusService statusService, IFollowersDal followersDal, InstanceSettings settings, ILogger<SendTweetsToInboxTask> logger, ISyncTweetsPostgresDal syncTweetsPostgresDal): base(syncTweetsPostgresDal)
        {
            _activityPubService = activityPubService;
            _statusService = statusService;
            _followersDal = followersDal;
            _settings = settings;
            _logger = logger;
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
                    try
                    {
                        if (!tweet.IsReply ||
                            tweet.IsReply && tweet.IsThread ||
                            _settings.PublishReplies)
                        {
                            var note = _statusService.GetStatus(user.Acct, tweet);
                            await _activityPubService.PostNewNoteActivity(note, user.Acct, tweet.Id.ToString(), follower.Host, inbox);
                            await SaveSyncTweetAsync(user.Acct, tweet.Id, follower.Host, inbox);
                        }
                    }
                    catch (ArgumentException e)
                    {
                        if (e.Message.Contains("Invalid pattern") && e.Message.Contains("at offset")) //Regex exception
                        {
                            _logger.LogError(e, "Can't parse {MessageContent} from Tweet {Id}", tweet.MessageContent, tweet.Id);
                        }
                        else
                        {
                            throw;
                        }
                    }

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