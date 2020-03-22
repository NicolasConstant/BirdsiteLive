using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Twitter;
using Microsoft.AspNetCore.Mvc;

namespace BirdsiteLive.Controllers
{
    public class UserController : Controller
    {
        private readonly ITwitterService _twitterService;

        #region Ctor
        public UserController(ITwitterService twitterService)
        {
            _twitterService = twitterService;
        }
        #endregion

        [Route("/@{id}")]
        [Route("/user/{id}")]
        public IActionResult Index(string id)
        {
            var user = _twitterService.GetUser(id);

            var r = Request.Headers["Accept"].First();

            if (r.Contains("application/activity+json"))
                return Json(new { test = "test" });

            return View(user);
        }
    }
}