using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BirdsiteLive.Controllers
{
    [ApiController]
    public class WellKnownController : ControllerBase
    {
        #region Ctor
        public WellKnownController()
        {

        }
        #endregion

        [Route("/.well-known/webfinger")]
        public WebFingerResult Webfinger(string resource = null)
        {
            var acct = resource.Split("acct:")[1];


            return new WebFingerResult()
            {
                subject = $"acct:{acct}",
                links = new List<WebFingerLink>
                {
                    new WebFingerLink()
                    {
                        rel = "self",
                        type = "application/activity+json",
                        href = "https://d150a079.ngrok.io/actor"
                    }
                }
            };
        }

        public class WebFingerResult
        {
            public string subject { get; set; }
            public List<WebFingerLink> links { get; set; } = new List<WebFingerLink>();
        }

        public class WebFingerLink
        {
            public string rel { get; set; }
            public string type { get; set; }
            public string href { get; set; }
        }
    }
}