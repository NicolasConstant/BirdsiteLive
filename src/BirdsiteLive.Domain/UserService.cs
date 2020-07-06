using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Cryptography;
using BirdsiteLive.Twitter.Models;
using Tweetinvi.Core.Exceptions;
using Tweetinvi.Models;

namespace BirdsiteLive.Domain
{
    public interface IUserService
    {
        Actor GetUser(TwitterUser twitterUser);
        Task<bool> FollowRequestedAsync(string signature, string method, string path, string queryString, Dictionary<string, string> requestHeaders, ActivityFollow activity);
        Note GetStatus(TwitterUser user, ITweet tweet);
    }

    public class UserService : IUserService
    {
        private readonly ICryptoService _cryptoService;
        private readonly IActivityPubService _activityPubService;
        private readonly string _host;

        #region Ctor
        public UserService(InstanceSettings instanceSettings, ICryptoService cryptoService, IActivityPubService activityPubService)
        {
            _cryptoService = cryptoService;
            _activityPubService = activityPubService;
            _host = $"https://{instanceSettings.Domain.Replace("https://",string.Empty).Replace("http://", string.Empty).TrimEnd('/')}";
        }
        #endregion

        public Actor GetUser(TwitterUser twitterUser)
        {
            var user = new Actor
            {
                id = $"{_host}/users/{twitterUser.Acct}",
                type = "Person",
                preferredUsername = twitterUser.Acct,
                name = twitterUser.Name,
                inbox = $"{_host}/users/{twitterUser.Acct}/inbox",
                summary = twitterUser.Description,
                url = $"{_host}/@{twitterUser.Acct}",
                publicKey = new PublicKey()
                {
                    id = $"{_host}/users/{twitterUser.Acct}#main-key",
                    owner = $"{_host}/users/{twitterUser.Acct}",
                    publicKeyPem = _cryptoService.GetUserPem(twitterUser.Acct)
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
                }
            };
            return user;
        }

        public Note GetStatus(TwitterUser user, ITweet tweet)
        {
            var actor = GetUser(user);

            var actorUrl = $"{_host}/users/{user.Acct}";
            var noteId = $"{_host}/users/{user.Acct}/statuses/{tweet.Id}";
            var noteUrl = $"{_host}/@{user.Acct}/{tweet.Id}";

            var to = $"{actor}/followers";
            var apPublic = "https://www.w3.org/ns/activitystreams#Public";

            var note = new Note
            {
                id = $"{noteId}/activity",
                
                published = tweet.CreatedAt.ToString("s") + "Z",
                url = noteUrl,
                attributedTo = actorUrl,

                //to = new [] {to},
                //cc = new [] { apPublic },

                to = new[] { apPublic },
                cc = new[] { to },

                sensitive = false,
                content = $"<p>{tweet.Text}</p>",
                attachment = new string[0],
                tag = new string[0]
            };
            return note;
        }

        public async Task<bool> FollowRequestedAsync(string signature, string method, string path, string queryString, Dictionary<string, string>  requestHeaders, ActivityFollow activity)
        {
            // Validate
            if (!await ValidateSignature(activity.actor, signature, method, path, queryString, requestHeaders)) return false;

            // Save Follow in DB
            
            // Send Accept Activity 
            var targetHost = activity.actor.Replace("https://", string.Empty).Split('/').First();
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
            var result = await _activityPubService.PostDataAsync(acceptFollow, targetHost, activity.apObject);
            return result == HttpStatusCode.Accepted;
        }
        
        private async Task<bool> ValidateSignature(string actor, string rawSig, string method, string path, string queryString, Dictionary<string, string> requestHeaders)
        {
            var signatures = rawSig.Split(',');
            var signature_header = new Dictionary<string, string>();
            foreach (var signature in signatures)
            {
                var splitSig = signature.Replace("\"", string.Empty).Split('=');
                signature_header.Add(splitSig[0], splitSig[1]);
            }

            signature_header["signature"] = signature_header["signature"] + "==";

            var key_id = signature_header["keyId"];
            var headers = signature_header["headers"];
            var algorithm = signature_header["algorithm"];
            var sig = Convert.FromBase64String(signature_header["signature"]);


            var remoteUser = await _activityPubService.GetUser(actor);

            var toDecode = remoteUser.publicKey.publicKeyPem.Trim().Remove(0, remoteUser.publicKey.publicKeyPem.IndexOf('\n'));
            toDecode = toDecode.Remove(toDecode.LastIndexOf('\n')).Replace("\n", "");
            var signKey = ASN1.ToRSA(Convert.FromBase64String(toDecode));

            var toSign = new StringBuilder();
            //var comparisonString = headers.Split(' ').Select(signed_header_name =>
            //{
            //    if (signed_header_name == "(request-target)")
            //        return "(request-target): post /inbox";
            //    else
            //        return $"{signed_header_name}: {r.Headers[signed_header_name.ToUpperInvariant()]}";
            //});

            foreach (var headerKey in headers.Split(' '))
            {
                if (headerKey == "(request-target)") toSign.Append($"(request-target): {method.ToLower()} {path}{queryString}\n");
                else toSign.Append($"{headerKey}: {string.Join(", ", requestHeaders[headerKey])}\n");
            }
            toSign.Remove(toSign.Length - 1, 1);

            //var signKey = ASN1.ToRSA(Convert.FromBase64String(toDecode));

            //new RSACryptoServiceProvider(keyId.publicKey.publicKeyPem);

            //Create a new instance of RSACryptoServiceProvider.
            RSACryptoServiceProvider key = new RSACryptoServiceProvider();

            //Get an instance of RSAParameters from ExportParameters function.
            RSAParameters RSAKeyInfo = key.ExportParameters(false);

            //Set RSAKeyInfo to the public key values.
            RSAKeyInfo.Modulus = Convert.FromBase64String(toDecode);

            key.ImportParameters(RSAKeyInfo);

            var result = signKey.VerifyData(Encoding.UTF8.GetBytes(toSign.ToString()), sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return result;
        }
    }
}
