using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BirdsiteLive.Component
{
    public class NodeInfoViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View(null);
        }
    }
}
