using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Converters;
using BirdsiteLive.Common.Regexes;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Cryptography;
using BirdsiteLive.Domain.BusinessUseCases;
using BirdsiteLive.Domain.Statistics;
using BirdsiteLive.Domain.Tools;
using BirdsiteLive.Twitter;
using BirdsiteLive.Twitter.Models;
using Tweetinvi.Core.Exceptions;
using Tweetinvi.Models;

namespace BirdsiteLive.Domain
{
    public interface IUserService
    {
        Actor GetUser(TwitterUser twitterUser);
        Task<bool> FollowRequestedAsync(string signature, string method, string path, string queryString, Dictionary<string, string> requestHeaders, ActivityFollow activity, string body);
        Task<bool> UndoFollowRequestedAsync(string signature, string method, string path, string queryString, Dictionary<string, string> requestHeaders, ActivityUndoFollow activity, string body);
    }

    public class UserService : IUserService
    {
        private readonly IProcessFollowUser _processFollowUser;
        private readonly IProcessUndoFollowUser _processUndoFollowUser;

        private readonly InstanceSettings _instanceSettings;
        private readonly ICryptoService _cryptoService;
        private readonly IActivityPubService _activityPubService;
        private readonly IStatusExtractor _statusExtractor;
        private readonly IExtractionStatisticsHandler _statisticsHandler;

        private readonly ITwitterUserService _twitterUserService;

        #region Ctor
        public UserService(InstanceSettings instanceSettings, ICryptoService cryptoService, IActivityPubService activityPubService, IProcessFollowUser processFollowUser, IProcessUndoFollowUser processUndoFollowUser, IStatusExtractor statusExtractor, IExtractionStatisticsHandler statisticsHandler, ITwitterUserService twitterUserService)
        {
            _instanceSettings = instanceSettings;
            _cryptoService = cryptoService;
            _activityPubService = activityPubService;
            _processFollowUser = processFollowUser;
            _processUndoFollowUser = processUndoFollowUser;
            _statusExtractor = statusExtractor;
            _statisticsHandler = statisticsHandler;
            _twitterUserService = twitterUserService;
        }
        #endregion

        public Actor GetUser(TwitterUser twitterUser)
        {
            var actorUrl = UrlFactory.GetActorUrl(_instanceSettings.Domain, twitterUser.Acct);
            var acct = twitterUser.Acct.ToLowerInvariant();

            // Extract links, mentions, etc
            var description = twitterUser.Description;
            if (!string.IsNullOrWhiteSpace(description))
            {
                var extracted = _statusExtractor.Extract(description, _instanceSettings.ResolveMentionsInProfiles);
                description = extracted.content;

                _statisticsHandler.ExtractedDescription(extracted.tags.Count(x => x.type == "Mention"));
            }

            var user = new Actor
            {
                id = actorUrl,
                type = "Service", 
                followers = $"{actorUrl}/followers",
                preferredUsername = acct,
                name = twitterUser.Name,
                inbox = $"{actorUrl}/inbox",
                summary = description,
                url = actorUrl,
                manuallyApprovesFollowers = twitterUser.Protected,
                publicKey = new PublicKey()
                {
                    id = $"{actorUrl}#main-key",
                    owner = actorUrl,
                    publicKeyPem = _cryptoService.GetUserPem(acct)
                },
                icon = new Image
                {
                    mediaType = "image/jpeg",
                    url = twitterUser.ProfileImageUrl
                },
                image = new Image
                {
                    mediaType = "image/jpeg",
                    url = twitterUser.ProfileBannerURL
                },
                attachment = new []
                {
                    new UserAttachment
                    {
                        type = "PropertyValue",
                        name = "Official",
                        value = $"<a href=\"https://twitter.com/{acct}\" rel=\"me nofollow noopener noreferrer\" target=\"_blank\"><span class=\"invisible\">https://</span><span class=\"ellipsis\">twitter.com/{acct}</span></a>"
                    }
                },
                endpoints = new EndPoints
                {
                    sharedInbox = $"https://{_instanceSettings.Domain}/inbox"
                }
            };
            return user;
        }

        public async Task<bool> FollowRequestedAsync(string signature, string method, string path, string queryString, Dictionary<string, string> requestHeaders, ActivityFollow activity, string body)
        {
            // Validate
            var sigValidation = await ValidateSignature(activity.actor, signature, method, path, queryString, requestHeaders, body);
            if (!sigValidation.SignatureIsValidated) return false;

            // Save Follow in DB
            var followerUserName = sigValidation.User.preferredUsername.ToLowerInvariant();
            var followerHost = sigValidation.User.url.Replace("https://", string.Empty).Split('/').First();
            var followerInbox = sigValidation.User.inbox;
            var followerSharedInbox = sigValidation.User?.endpoints?.sharedInbox;
            var twitterUser = activity.apObject.Split('/').Last().Replace("@", string.Empty);

            // Make sure to only keep routes
            followerInbox = OnlyKeepRoute(followerInbox, followerHost);
            followerSharedInbox = OnlyKeepRoute(followerSharedInbox, followerHost);
            
            var user = _twitterUserService.GetUser(twitterUser);
            if (!user.Protected)
            {
                // Execute
                await _processFollowUser.ExecuteAsync(followerUserName, followerHost, twitterUser, followerInbox, followerSharedInbox);

                // Send Accept Activity
                var acceptFollow = new ActivityAcceptFollow()
                {
                    context = "https://www.w3.org/ns/activitystreams",
                    id = $"{activity.apObject}#accepts/follows/{Guid.NewGuid()}",
                    type = "Accept",
                    actor = activity.apObject,
                    apObject = new ActivityFollow()
                    {
                        id = activity.id,
                        type = activity.type,
                        actor = activity.actor,
                        apObject = activity.apObject
                    }
                };
                var result = await _activityPubService.PostDataAsync(acceptFollow, followerHost, activity.apObject);
                return result == HttpStatusCode.Accepted || result == HttpStatusCode.OK; //TODO: revamp this for better error handling
            }
            else
            {
                // Send Reject Activity
                var acceptFollow = new ActivityRejectFollow()
                {
                    context = "https://www.w3.org/ns/activitystreams",
                    id = $"{activity.apObject}#rejects/follows/{Guid.NewGuid()}",
                    type = "Reject",
                    actor = activity.apObject,
                    apObject = new ActivityFollow()
                    {
                        id = activity.id,
                        type = activity.type,
                        actor = activity.actor,
                        apObject = activity.apObject
                    }
                };
                var result = await _activityPubService.PostDataAsync(acceptFollow, followerHost, activity.apObject);
                return result == HttpStatusCode.Accepted || result == HttpStatusCode.OK; //TODO: revamp this for better error handling
            }
        }

        private string OnlyKeepRoute(string inbox, string host)
        {
            if (string.IsNullOrWhiteSpace(inbox)) 
                return null;

            if (inbox.Contains(host))
                inbox = inbox.Split(new[] { host }, StringSplitOptions.RemoveEmptyEntries).Last();

            return inbox;
        }

        public async Task<bool> UndoFollowRequestedAsync(string signature, string method, string path, string queryString,
            Dictionary<string, string> requestHeaders, ActivityUndoFollow activity, string body)
        {
            // Validate
            var sigValidation = await ValidateSignature(activity.actor, signature, method, path, queryString, requestHeaders, body);
            if (!sigValidation.SignatureIsValidated) return false;

            // Save Follow in DB
            var followerUserName = sigValidation.User.preferredUsername.ToLowerInvariant();
            var followerHost = sigValidation.User.url.Replace("https://", string.Empty).Split('/').First();
            //var followerInbox = sigValidation.User.inbox;
            var twitterUser = activity.apObject.apObject.Split('/').Last().Replace("@", string.Empty);
            await _processUndoFollowUser.ExecuteAsync(followerUserName, followerHost, twitterUser);

            // Send Accept Activity
            var acceptFollow = new ActivityAcceptUndoFollow()
            {
                context = "https://www.w3.org/ns/activitystreams",
                id = $"{activity.apObject.apObject}#accepts/undofollows/{Guid.NewGuid()}",
                type = "Accept",
                actor = activity.apObject.apObject,
                apObject = new ActivityUndoFollow()
                {
                    id = activity.id,
                    type = activity.type,
                    actor = activity.actor,
                    apObject = activity.apObject
                }
            };
            var result = await _activityPubService.PostDataAsync(acceptFollow, followerHost, activity.apObject.apObject);
            return result == HttpStatusCode.Accepted || result == HttpStatusCode.OK; //TODO: revamp this for better error handling
        }

        private async Task<SignatureValidationResult> ValidateSignature(string actor, string rawSig, string method, string path, string queryString, Dictionary<string, string> requestHeaders, string body)
        {
            //Check Date Validity
            var date = requestHeaders["date"];
            var d = DateTime.Parse(date).ToUniversalTime();
            var now = DateTime.UtcNow;
            var delta = Math.Abs((d - now).TotalSeconds);
            if (delta > 30) return new SignatureValidationResult { SignatureIsValidated = false };
            
            //Check Digest
            var digest = requestHeaders["digest"];
            var digestHash = digest.Split(new [] {"SHA-256="},StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            var calculatedDigestHash = _cryptoService.ComputeSha256Hash(body);
            if (digestHash != calculatedDigestHash) return new SignatureValidationResult { SignatureIsValidated = false };

            //Check Signature
            var signatures = rawSig.Split(',');
            var signature_header = new Dictionary<string, string>();
            foreach (var signature in signatures)
            {
                var m = HeaderRegexes.HeaderSignature.Match(signature);
                signature_header.Add(m.Groups[1].ToString(), m.Groups[2].ToString());
            }

            var key_id = signature_header["keyId"];
            var headers = signature_header["headers"];
            var algorithm = signature_header["algorithm"];
            var sig = Convert.FromBase64String(signature_header["signature"]);

            // Retrieve User
            var remoteUser = await _activityPubService.GetUser(actor);

            // Prepare Key data
            var toDecode = remoteUser.publicKey.publicKeyPem.Trim().Remove(0, remoteUser.publicKey.publicKeyPem.IndexOf('\n'));
            toDecode = toDecode.Remove(toDecode.LastIndexOf('\n')).Replace("\n", "");
            var signKey = ASN1.ToRSA(Convert.FromBase64String(toDecode));

            var toSign = new StringBuilder();
            foreach (var headerKey in headers.Split(' '))
            {
                if (headerKey == "(request-target)") toSign.Append($"(request-target): {method.ToLower()} {path}{queryString}\n");
                else toSign.Append($"{headerKey}: {string.Join(", ", requestHeaders[headerKey])}\n");
            }
            toSign.Remove(toSign.Length - 1, 1);

            // Import key
            var key = new RSACryptoServiceProvider();
            var rsaKeyInfo = key.ExportParameters(false);
            rsaKeyInfo.Modulus = Convert.FromBase64String(toDecode);
            key.ImportParameters(rsaKeyInfo);

            // Trust and Verify
            var result = signKey.VerifyData(Encoding.UTF8.GetBytes(toSign.ToString()), sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return new SignatureValidationResult()
            {
                SignatureIsValidated = result,
                User = remoteUser
            };
        }
    }

    public class SignatureValidationResult 
    {
        public bool SignatureIsValidated { get; set; }
        public Actor User { get; set; }
    }
}
