using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Models;
using BirdsiteLive.Models.WellKnownModels;
using BirdsiteLive.Twitter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BirdsiteLive.Controllers
{
    [ApiController]
    public class WellKnownController : ControllerBase
    {
        private readonly ITwitterService _twitterService;
        private readonly InstanceSettings _settings;

        #region Ctor
        public WellKnownController(InstanceSettings settings, ITwitterService twitterService)
        {
            _twitterService = twitterService;
            _settings = settings;
        }
        #endregion

        [Route("/.well-known/nodeinfo")]
        public IActionResult WellKnownNodeInfo()
        {
            var nodeInfo = new WellKnownNodeInfo
            {
                links = new Link[]
                {
                    new Link()
                    {
                        rel = "http://nodeinfo.diaspora.software/ns/schema/2.0",
                        href = $"https://{_settings.Domain}/nodeinfo/2.0.json"
                    },
                    new Link()
                    {
                        rel = "http://nodeinfo.diaspora.software/ns/schema/2.1",
                        href = $"https://{_settings.Domain}/nodeinfo/2.1.json"
                    }
                }
            };
            return new JsonResult(nodeInfo);
        }

        [Route("/nodeinfo/{id}.json")]
        public IActionResult NodeInfo(string id)
        {
            var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString(3);

            if (id == "2.0")
            {
                var nodeInfo = new NodeInfoV20
                {
                    version = "2.0",
                    usage = new Usage()
                    {
                        localPosts = 0,
                        users = new Users()
                        {
                            total = 0
                        }
                    },
                    software = new Software()
                    {
                        name = "birdsitelive",
                        version = version
                    },
                    protocols = new[]
                    {
                        "activitypub"
                    },
                    openRegistrations = false,
                    services = new Models.WellKnownModels.Services()
                    {
                        inbound = new object[0],
                        outbound = new object[0]
                    },
                    metadata = new Metadata()
                    {
                        email = _settings.AdminEmail
                    }
                };
                return new JsonResult(nodeInfo);
            }
            if (id == "2.1")
            {
                var nodeInfo = new NodeInfoV21
                {
                    version = "2.1",
                    usage = new Usage()
                    {
                        localPosts = 0,
                        users = new Users()
                        {
                            total = 0
                        }
                    },
                    software = new SoftwareV21()
                    {
                        name = "birdsitelive",
                        version = version,
                        repository = "https://github.com/NicolasConstant/BirdsiteLive"
                    },
                    protocols = new[]
                    {
                        "activitypub"
                    },
                    openRegistrations = false,
                    services = new Models.WellKnownModels.Services()
                    {
                        inbound = new object[0],
                        outbound = new object[0]
                    },
                    metadata = new Metadata()
                    {
                        email = _settings.AdminEmail
                    }
                };
                return new JsonResult(nodeInfo);
            }

            return NotFound();
        }

        [Route("/.well-known/webfinger")]
        public IActionResult Webfinger(string resource = null)
        {
            var acct = resource.Split("acct:")[1].Trim();

            string name = null;
            string domain = null;

            var splitAcct = acct.Split('@', StringSplitOptions.RemoveEmptyEntries);

            var atCount = acct.Count(x => x == '@');
            if (atCount == 1 && acct.StartsWith('@'))
            {
                name = splitAcct[1];
            }
            else if (atCount == 1 || atCount == 2)
            {
                name = splitAcct[0];
                domain = splitAcct[1];
            }
            else
            {
                return BadRequest();
            }

            if (!string.IsNullOrWhiteSpace(domain) && domain != _settings.Domain)
                return NotFound();

            var user = _twitterService.GetUser(name);
            if (user == null)
                return NotFound();

            var result = new WebFingerResult()
            {
                subject = $"acct:{name}@{_settings.Domain}",
                aliases = new[]
                {
                    $"https://{_settings.Domain}/@{name}",
                    $"https://{_settings.Domain}/users/{name}"
                },
                links = new List<WebFingerLink>
                {
                    new WebFingerLink()
                    {
                        rel = "http://webfinger.net/rel/profile-page",
                        type = "text/html",
                        href = $"https://{_settings.Domain}/@{name}"
                    },
                    new WebFingerLink()
                    {
                        rel = "self",
                        type = "application/activity+json",
                        href = $"https://{_settings.Domain}/users/{name}"
                    }
                }
            };

            return new JsonResult(result);
        }
    }
}