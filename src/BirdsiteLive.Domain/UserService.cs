using System;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Twitter.Models;

namespace BirdsiteLive.Domain
{
    public interface IUserService
    {
        Actor GetUser(TwitterUser twitterUser);
    }

    public class UserService : IUserService
    {
        private readonly ICryptoService _cryptoService;
        private readonly string _host;

        #region Ctor
        public UserService(InstanceSettings instanceSettings, ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
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
    }
}
