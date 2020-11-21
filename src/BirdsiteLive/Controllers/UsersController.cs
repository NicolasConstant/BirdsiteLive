using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain;
using BirdsiteLive.Models;
using BirdsiteLive.Twitter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace BirdsiteLive.Controllers
{
    public class UsersController : Controller
    {
        private readonly ITwitterService _twitterService;
        private readonly IUserService _userService;
        private readonly IStatusService _statusService;
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public UsersController(ITwitterService twitterService, IUserService userService, IStatusService statusService, InstanceSettings instanceSettings)
        {
            _twitterService = twitterService;
            _userService = userService;
            _statusService = statusService;
            _instanceSettings = instanceSettings;
        }
        #endregion

        [Route("/@{id}")]
        [Route("/users/{id}")]
        public IActionResult Index(string id)
        {
            var user = _twitterService.GetUser(id);
            if (user == null) return NotFound();

            var r = Request.Headers["Accept"].First();
            if (r.Contains("application/activity+json"))
            {
                var apUser = _userService.GetUser(user);
                var jsonApUser = JsonConvert.SerializeObject(apUser);
                return Content(jsonApUser, "application/activity+json; charset=utf-8");
            }

            var displayableUser = new DisplayTwitterUser
            {
                Name = user.Name,
                Description = user.Description,
                Acct = user.Acct,
                Url = user.Url,
                ProfileImageUrl = user.ProfileImageUrl,

                InstanceHandle = $"@{user.Acct}@{_instanceSettings.Domain}"
            };
            return View(displayableUser);
        }

        [Route("/@{id}/{statusId}")]
        [Route("/users/{id}/statuses/{statusId}")]
        public IActionResult Tweet(string id, string statusId)
        {
            var r = Request.Headers["Accept"].First();
            if (r.Contains("application/activity+json"))
            {
                if (!long.TryParse(statusId, out var parsedStatusId))
                    return NotFound();

                var tweet = _twitterService.GetTweet(parsedStatusId);
                if (tweet == null)
                    return NotFound();

                //var user = _twitterService.GetUser(id);
                //if (user == null) return NotFound();

                var status = _statusService.GetStatus(id, tweet);
                var jsonApUser = JsonConvert.SerializeObject(status);
                return Content(jsonApUser, "application/activity+json; charset=utf-8");
            }

            return View("Tweet", statusId);
        }

        [Route("/users/{id}/inbox")]
        [HttpPost]
        public async Task<IActionResult> Inbox()
        {
            var r = Request;
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                //System.IO.File.WriteAllText($@"C:\apdebug\{Guid.NewGuid()}.json", body);

                var activity = ApDeserializer.ProcessActivity(body);
                // Do something
                var signature = r.Headers["Signature"].First();

                Console.WriteLine(body);
                Console.WriteLine();

                switch (activity?.type)
                {
                    case "Follow":
                        {
                            var succeeded = await _userService.FollowRequestedAsync(signature, r.Method, r.Path,
                                r.QueryString.ToString(), RequestHeaders(r.Headers), activity as ActivityFollow);
                            if (succeeded) return Accepted();
                            else return Unauthorized();
                        }
                    case "Undo":
                        if (activity is ActivityUndoFollow)
                        {
                            var succeeded = await _userService.UndoFollowRequestedAsync(signature, r.Method, r.Path,
                                r.QueryString.ToString(), RequestHeaders(r.Headers), activity as ActivityUndoFollow);
                            if (succeeded) return Accepted();
                            else return Unauthorized();
                        }
                        return Accepted();
                    default:
                        return Accepted();
                }
            }

            return Accepted();
        }

        [Route("/users/{id}/followers")]
        [HttpGet]
        public async Task<IActionResult> Followers(string id)
        {
            var r = Request.Headers["Accept"].First();
            if (!r.Contains("application/activity+json")) return NotFound();

            var followers = new Followers
            {
                id = $"https://{_instanceSettings.Domain}/users/{id}/followers"
            };
            var jsonApUser = JsonConvert.SerializeObject(followers);
            return Content(jsonApUser, "application/activity+json; charset=utf-8");
        }

        private Dictionary<string, string> RequestHeaders(IHeaderDictionary header)
        {
            return header.ToDictionary<KeyValuePair<string, StringValues>, string, string>(h => h.Key.ToLowerInvariant(), h => h.Value);
        }
    }
}