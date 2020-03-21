using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BirdsiteLive.Controllers
{
    [ApiController]
    public class InboxController : ControllerBase
    {
        [Route("/inbox")]
        [HttpPost]
        public async Task<IActionResult> Inbox()
        {
            throw new NotImplementedException();
        }
    }
}