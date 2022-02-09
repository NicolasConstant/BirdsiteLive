using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Controllers
{
    [ApiController]
    public class InboxController : ControllerBase
    {
        private readonly ILogger<InboxController> _logger;
        private readonly IUserService _userService;

        #region Ctor
        public InboxController(ILogger<InboxController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }
        #endregion

        [Route("/inbox")]
        [HttpPost]
        public async Task<IActionResult> Inbox()
        {
            try
            {
                var r = Request;
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();

                    _logger.LogTrace("Inbox: {Body}", body);
                    //System.IO.File.WriteAllText($@"C:\apdebug\inbox\{Guid.NewGuid()}.json", body);

                    var activity = ApDeserializer.ProcessActivity(body);
                    var signature = r.Headers["Signature"].First();

                    switch (activity?.type)
                    {
                        case "Delete":
                        {
                            var succeeded = await _userService.DeleteRequestedAsync(signature, r.Method, r.Path,
                                r.QueryString.ToString(), HeaderHandler.RequestHeaders(r.Headers), activity as ActivityDelete, body);
                            if (succeeded) return Accepted();
                            else return Unauthorized();
                        }
                    }
                }
            }
            catch (FollowerIsGoneException) { } //TODO: check if user in DB

            return Accepted();
        }
    }
}