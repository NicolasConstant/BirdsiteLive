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
    #if DEBUG
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
            var username = "twitter";
            var actor = $"https://{_instanceSettings.Domain}/users/{username}";
            var targetHost = "ioc.exchange";
            var target = $"https://{targetHost}/users/test";
            //var inbox = $"/users/testtest/inbox";
            var inbox = $"/inbox";

            var noteGuid = Guid.NewGuid();
            var noteId = $"https://{_instanceSettings.Domain}/users/{username}/statuses/{noteGuid}";
            var noteUrl = $"https://{_instanceSettings.Domain}/@{username}/{noteGuid}";

            var to = $"{actor}/followers";
            to = target;

            var cc = new[] { "https://www.w3.org/ns/activitystreams#Public" };
            cc = new string[0];

            var now = DateTime.UtcNow;
            var nowString = now.ToString("s") + "Z";

            var noteActivity = new ActivityCreateNote()
            {
                context = "https://www.w3.org/ns/activitystreams",
                id = $"{noteId}/activity",
                type = "Create",
                actor = actor,
                published = nowString,
                to = new[] { to },
                cc = cc,
                apObject = new Note()
                {
                    id = noteId,
                    summary = null,
                    inReplyTo = null,
                    published = nowString,
                    url = noteUrl,
                    attributedTo = actor,

                    // Unlisted
                    to = new[] { to },
                    cc = cc,
                    //cc = new[] { "https://www.w3.org/ns/activitystreams#Public" },

                    //// Public
                    //to = new[] { "https://www.w3.org/ns/activitystreams#Public" },
                    //cc = new[] { to },

                    sensitive = false,
                    content = "<p>TEST PUBLIC</p>",
                    //content = "<p><span class=\"h-card\"><a href=\"https://ioc.exchange/users/test\" class=\"u-url mention\">@<span>test</span></a></span> test</p>",
                    attachment = new Attachment[0],
                    tag = new Tag[]{
                        new Tag()
                        {
                            type = "Mention",
                            href = target,
                            name = "@test@ioc.exchange"
                        }
                    },
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

        [HttpPost]
        public async Task<IActionResult> PostDeleteUser()
        {
            var userName = "twitter";
            var host = "ioc.exchange";
            var inbox = "/inbox";

            await _activityPubService.DeleteUserAsync(userName, host, inbox);
            return View("Index");
        }
    }
    #endif

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