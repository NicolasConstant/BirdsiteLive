using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Controllers
{
    [ApiController]
    public class InboxController : ControllerBase
    {
        private readonly ILogger<InboxController> _logger;

        #region Ctor
        public InboxController(ILogger<InboxController> logger)
        {
            _logger = logger;
        }
        #endregion

        [Route("/inbox")]
        [HttpPost]
        public async Task<IActionResult> Inbox()
        {
            var r = Request;
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();

                _logger.LogTrace("Inbox: {Body}", body);
                //System.IO.File.WriteAllText($@"C:\apdebug\inbox\{Guid.NewGuid()}.json", body);

            }

            return Accepted();
        }
    }
}