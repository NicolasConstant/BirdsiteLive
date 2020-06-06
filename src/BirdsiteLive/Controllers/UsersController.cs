using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Domain;
using BirdsiteLive.Twitter;
using Microsoft.AspNetCore.Mvc;

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
                return Json(apUser);
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

                // Do something
            }

            return Ok();
        }
    }
}