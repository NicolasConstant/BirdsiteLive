using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace BirdsiteLive.Controllers
{
    public class DebugController : Controller
    {
        private readonly InstanceSettings _instanceSettings;
        private readonly ICryptoService _cryptoService;

        #region Ctor
        public DebugController(InstanceSettings instanceSettings, ICryptoService cryptoService)
        {
            _instanceSettings = instanceSettings;
            _cryptoService = cryptoService;
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Follow()
        {
            var actor = $"https://{_instanceSettings.Domain}/users/gra";
            var targethost = "mamot.fr";
            var followActivity = new ActivityFollow()
            {
                context = "https://www.w3.org/ns/activitystreams",
                id = $"https://{_instanceSettings.Domain}/{Guid.NewGuid()}",
                type = "Follow",
                actor = actor,
                apObject = $"https://{targethost}/users/testtest"
            };

            var json = JsonConvert.SerializeObject(followActivity);
            
            var date = DateTime.UtcNow.ToUniversalTime();
            var httpDate = date.ToString("r");
            var signature = _cryptoService.SignAndGetSignatureHeader(date, actor, targethost);

            var client = new HttpClient();
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://{targethost}/inbox"),
                Headers =
                {
                    {"Host", targethost},
                    {"Date", httpDate},
                    {"Signature", signature}
                },
                Content = new StringContent(json, Encoding.UTF8, "application/ld+json")
            };

            try
            {
                var response = await client.SendAsync(httpRequestMessage);
                var re = response.ReasonPhrase;
                var t = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw;
            }

            return View("Index");
        }
    }

    public static class HtmlHelperExtensions
    {
        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}