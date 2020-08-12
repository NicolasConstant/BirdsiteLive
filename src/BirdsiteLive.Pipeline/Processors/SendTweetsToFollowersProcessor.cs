using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Twitter;
using BirdsiteLive.Twitter.Models;
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

            // Process Shared Inbox
            var followersWtSharedInbox = userWithTweetsToSync.Followers
                .Where(x => !string.IsNullOrWhiteSpace(x.SharedInboxRoute))
                .ToList();
            await ProcessFollowersWithSharedInbox(userWithTweetsToSync.Tweets, followersWtSharedInbox, user);

            // Process Inbox
            var followerWtInbox = userWithTweetsToSync.Followers
                .Where(x => string.IsNullOrWhiteSpace(x.SharedInboxRoute))
                .ToList();
            await ProcessFollowersWithInbox(userWithTweetsToSync.Tweets, followerWtInbox, user);

            return userWithTweetsToSync;
        }

        private async Task ProcessFollowersWithSharedInbox(ExtractedTweet[] tweets, List<Follower> followers, SyncTwitterUser user)
        {
            var followersPerInstances = followers.GroupBy(x => x.Host);

            foreach (var followersPerInstance in followersPerInstances)
            {
                try
                {
                    await ProcessInstanceFollowersWithSharedInbox(tweets, user, followersPerInstance);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //TODO handle error
                }
            }
        }

        private async Task ProcessInstanceFollowersWithSharedInbox(ExtractedTweet[] tweets, SyncTwitterUser user,
            IGrouping<string, Follower> followersPerInstance)
        {
            var userId = user.Id;
            var host = followersPerInstance.Key;
            var groupedFollowers = followersPerInstance.ToList();
            var inbox = groupedFollowers.First().SharedInboxRoute;

            var fromStatusId = groupedFollowers
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
                    foreach (var f in groupedFollowers)
                    {
                        f.FollowingsSyncStatus[userId] = syncStatus;
                        await _followersDal.UpdateFollowerAsync(f);
                    }
                }
            }
        }

        private async Task ProcessFollowersWithInbox(ExtractedTweet[] tweets, List<Follower> followerWtInbox, SyncTwitterUser user)
        {
            foreach (var follower in followerWtInbox)
            {
                try
                {
                    await ProcessFollowerWithInboxAsync(tweets, follower, user);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //TODO handle error
                }
            }
        }

        private async Task ProcessFollowerWithInboxAsync(IEnumerable<ExtractedTweet> tweets, Follower follower, SyncTwitterUser user)
        {
            var userId = user.Id;
            var fromStatusId = follower.FollowingsSyncStatus[userId];
            var tweetsToSend = tweets
                .Where(x => x.Id > fromStatusId)
                .OrderBy(x => x.Id)
                .ToList();

            var inbox = follower.InboxRoute;
            //var inbox = string.IsNullOrWhiteSpace(follower.SharedInboxRoute)
            //    ? follower.InboxRoute
            //    : follower.SharedInboxRoute;

            var syncStatus = fromStatusId;
            try
            {
                foreach (var tweet in tweetsToSend)
                {
                    var note = _statusService.GetStatus(user.Acct, tweet);
                    var result = await _activityPubService.PostNewNoteActivity(note, user.Acct, tweet.Id.ToString(), follower.Host, inbox);

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