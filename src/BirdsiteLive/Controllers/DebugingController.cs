using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace BirdsiteLive.Controllers
{
    public class DebugingController : Controller
    {
        private readonly InstanceSettings _instanceSettings;
        private readonly ICryptoService _cryptoService;
        private readonly IActivityPubService _activityPubService;
        private readonly IUserService _userService;

        #region Ctor
        public DebugingController(InstanceSettings instanceSettings, ICryptoService cryptoService, IActivityPubService activityPubService, IUserService userService)
        {
            _instanceSettings = instanceSettings;
            _cryptoService = cryptoService;
            _activityPubService = activityPubService;
            _userService = userService;
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Follow()
        {
            var actor = $"https://{_instanceSettings.Domain}/users/gra";
            var targethost = "mastodon.technology";
            var followActivity = new ActivityFollow()
            {
                context = "https://www.w3.org/ns/activitystreams",
                id = $"https://{_instanceSettings.Domain}/{Guid.NewGuid()}",
                type = "Follow",
                actor = actor,
                apObject = $"https://{targethost}/users/testtest"
            };

            await _activityPubService.PostDataAsync(followActivity, targethost, actor);

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PostNote()
        {
            var username = "gra";
            var actor = $"https://{_instanceSettings.Domain}/users/{username}";
            var targetHost = "mastodon.technology";
            var target = $"{targetHost}/users/testtest";
            var inbox = $"/users/testtest/inbox";

            var noteGuid = Guid.NewGuid();
            var noteId = $"https://{_instanceSettings.Domain}/users/{username}/statuses/{noteGuid}";
            var noteUrl = $"https://{_instanceSettings.Domain}/@{username}/{noteGuid}";
            
            var to = $"{actor}/followers";
            var apPublic = "https://www.w3.org/ns/activitystreams#Public";

            var now = DateTime.UtcNow;
            var nowString = now.ToString("s") + "Z";

            var noteActivity = new ActivityCreateNote()
            {
                context = "https://www.w3.org/ns/activitystreams",
                id = $"{noteId}/activity",
                type = "Create",
                actor = actor,
                published = nowString,
                to = new []{ to },
                //cc = new [] { apPublic },
                apObject = new Note()
                {
                    id = noteId,
                    summary = null, 
                    inReplyTo = null,
                    published = nowString,
                    url = noteUrl,
                    attributedTo = actor,
                    to = new[] { to },
                    //cc = new [] { apPublic },
                    sensitive = false,
                    content = "<p>Woooot</p>",
                    attachment = new Attachment[0],
                    tag = new Tag[0]
                }
            };

            await _activityPubService.PostDataAsync(noteActivity, targetHost, actor, inbox);

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PostRejectFollow()
        {
            var activityFollow = new ActivityFollow
            {
                type = "Follow",
                actor = "https://mastodon.technology/users/testtest",
                apObject = $"https://{_instanceSettings.Domain}/users/afp"
            };

            await _userService.SendRejectFollowAsync(activityFollow, "mastodon.technology");
            return View("Index");
        }
    }

    public static class HtmlHelperExtensions
    {
        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}