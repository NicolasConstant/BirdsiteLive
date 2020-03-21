using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BirdsiteLive.Controllers
{
    public class UserController : Controller
    {
        [Route("/user")]
        public IActionResult Index()
        {
            var r = Request.Headers["Accept"].First();

            if(r.Contains("application/activity+json"))
                return Json(new { test = "test" });
                
            return View();
        }
    }
}