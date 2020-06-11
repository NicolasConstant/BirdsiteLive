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
                return Content(jsonApUser, "application/json");
            }

            return View(user);
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

                switch (activity.type)
                {
                    case "Follow": 
                        var succeeded = await _userService.FollowRequestedAsync(r.Headers["Signature"].First(), r.Method, r.Path, r.QueryString.ToString(), RequestHeaders(r.Headers), activity as ActivityFollow);
                        if (succeeded) return Ok();
                        else return Unauthorized();
                        break;
                    default:
                        return Ok();
                }
            }

            return Ok();
        }

        private Dictionary<string, string> RequestHeaders(IHeaderDictionary header)
        {
            return header.ToDictionary<KeyValuePair<string, StringValues>, string, string>(h => h.Key.ToLowerInvariant(), h => h.Value);
        }
    }
}