using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BirdsiteLive.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Blacklisting()
        {
            return View("Blacklisting");
        }

        public IActionResult Whitelisting()
        {
            return View("Whitelisting");
        }
    }
}
