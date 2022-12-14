﻿using System;
using System.Linq;
using BirdsiteLive.Twitter;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.ActivityPub.Converters;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain.Enum;

namespace BirdsiteLive.Domain
{
    public class MigrationService
    {
        private readonly InstanceSettings _instanceSettings;
        private readonly ITwitterTweetsService _twitterTweetsService;
        private readonly IActivityPubService _activityPubService;
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly IFollowersDal _followersDal;

        #region Ctor
        public MigrationService(ITwitterTweetsService twitterTweetsService, IActivityPubService activityPubService, ITwitterUserDal twitterUserDal, IFollowersDal followersDal, InstanceSettings instanceSettings)
        {
            _twitterTweetsService = twitterTweetsService;
            _activityPubService = activityPubService;
            _twitterUserDal = twitterUserDal;
            _followersDal = followersDal;
            _instanceSettings = instanceSettings;
        }
        #endregion

        public string GetMigrationCode(string acct)
        {
            var hash = GetHashString(acct);
            return $"[[BirdsiteLIVE-MigrationCode|{hash.Substring(0, 10)}]]";
        }

        public string GetDeletionCode(string acct)
        {
            var hash = GetHashString(acct);
            return $"[[BirdsiteLIVE-DeletionCode|{hash.Substring(0, 10)}]]";
        }

        public bool ValidateTweet(string acct, string tweetId, MigrationTypeEnum type)
        {
            string code;
            if (type == MigrationTypeEnum.Migration)
                code = GetMigrationCode(acct);
            else if (type == MigrationTypeEnum.Deletion)
                code = GetDeletionCode(acct);
            else
                throw new NotImplementedException();

            var castedTweetId = ExtractedTweetId(tweetId);
            var tweet = _twitterTweetsService.GetTweet(castedTweetId);

            if (tweet == null)
                throw new Exception("Tweet not found");

            if (tweet.CreatorName.Trim().ToLowerInvariant() != acct.Trim().ToLowerInvariant())
                throw new Exception($"Tweet not published by @{acct}");

            if (!tweet.MessageContent.Contains(code))
                throw new Exception("Tweet don't have migration code");

            return true;
        }

        private long ExtractedTweetId(string tweetId)
        {
            long castedId;
            if (long.TryParse(tweetId, out castedId))
                return castedId;

            var urlPart = tweetId.Split('/').LastOrDefault();
            if (long.TryParse(urlPart, out castedId))
                return castedId;

            throw new ArgumentException("Unvalid Tweet ID");
        }

        public async Task<ValidatedFediverseUser> ValidateFediverseAcctAsync(string fediverseAcct)
        {
            if (string.IsNullOrWhiteSpace(fediverseAcct))
                throw new ArgumentException("Please provide Fediverse account");

            if (!fediverseAcct.Contains('@') || !fediverseAcct.StartsWith("@") || fediverseAcct.Trim('@').Split('@').Length != 2)
                throw new ArgumentException("Please provide valid Fediverse handle");

            var objectId = await _activityPubService.GetUserIdAsync(fediverseAcct);
            var user = await _activityPubService.GetUser(objectId);

            var result = new ValidatedFediverseUser
            {
                FediverseAcct = fediverseAcct,
                ObjectId = objectId,
                User = user,
                IsValid = user != null
            };

            return result;
        }

        public async Task MigrateAccountAsync(ValidatedFediverseUser validatedUser, string acct)
        {
            // Apply moved to
            var twitterAccount = await _twitterUserDal.GetTwitterUserAsync(acct);
            if (twitterAccount == null)
            {
                await _twitterUserDal.CreateTwitterUserAsync(acct, -1, validatedUser.ObjectId, validatedUser.FediverseAcct);
                twitterAccount = await _twitterUserDal.GetTwitterUserAsync(acct);
            }

            twitterAccount.MovedTo = validatedUser.ObjectId;
            twitterAccount.MovedToAcct = validatedUser.FediverseAcct;
            await _twitterUserDal.UpdateTwitterUserAsync(twitterAccount);

            // Notify Followers
            var message = $@"<p>[BSL MIRROR SERVICE NOTIFICATION]<br/>
                                    This bot has been disabled by it's original owner.<br/>
                                    It has been redirected to {validatedUser.FediverseAcct}.
                                    </p>";
            NotifyFollowers(acct, twitterAccount, message);
        }

        private void NotifyFollowers(string acct, SyncTwitterUser twitterAccount, string message)
        {
            var t = Task.Run(async () =>
            {
                var followers = await _followersDal.GetFollowersAsync(twitterAccount.Id);
                foreach (var follower in followers)
                {
                    try
                    {
                        var noteId = Guid.NewGuid().ToString();
                        var actorUrl = UrlFactory.GetActorUrl(_instanceSettings.Domain, acct);
                        var noteUrl = UrlFactory.GetNoteUrl(_instanceSettings.Domain, acct, noteId);

                        //var to = validatedUser.ObjectId;
                        var to = follower.ActorId;
                        var cc = new string[0];

                        var note = new Note
                        {
                            id = noteId,

                            published = DateTime.UtcNow.ToString("s") + "Z",
                            url = noteUrl,
                            attributedTo = actorUrl,

                            to = new[] { to },
                            cc = cc,

                            content = message
                        };

                        await _activityPubService.PostNewNoteActivity(note, acct, Guid.NewGuid().ToString(), follower.Host,
                            follower.InboxRoute);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }

        public async Task DeleteAccountAsync(string acct)
        {
            // Apply moved to
            var twitterAccount = await _twitterUserDal.GetTwitterUserAsync(acct);
            if (twitterAccount == null)
            {
                await _twitterUserDal.CreateTwitterUserAsync(acct, -1);
                twitterAccount = await _twitterUserDal.GetTwitterUserAsync(acct);
            }

            twitterAccount.Deleted = true;
            await _twitterUserDal.UpdateTwitterUserAsync(twitterAccount);


            // Notify Followers
            var message = $@"<p>[BSL MIRROR SERVICE NOTIFICATION]<br/>
                                    This bot has been deleted by it's original owner.<br/>
                                    </p>";
            NotifyFollowers(acct, twitterAccount, message);
        }

        public async Task TriggerRemoteMigrationAsync(string id, string tweetid, string handle)
        {
            //TODO
        }

        public async Task TriggerRemoteDeleteAsync(string id, string tweetid)
        {
            //TODO
        }

        private byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }

    public class ValidatedFediverseUser
    {
        public string FediverseAcct { get; set; }
        public string ObjectId { get; set; }
        public Actor User { get; set; }
        public bool IsValid { get; set; }
    }
}