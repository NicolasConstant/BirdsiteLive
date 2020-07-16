using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.Domain;
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

        #region Ctor
        public UsersController(ITwitterService twitterService, IUserService userService)
        {
            _twitterService = twitterService;
            _userService = userService;
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

            return View(user);
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

                var user = _twitterService.GetUser(id);
                if (user == null) return NotFound();

                var status = _userService.GetStatus(user, tweet);
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

        private Dictionary<string, string> RequestHeaders(IHeaderDictionary header)
        {
            return header.ToDictionary<KeyValuePair<string, StringValues>, string, string>(h => h.Key.ToLowerInvariant(), h => h.Value);
        }
    }
}