using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;

namespace BirdsiteLive.Domain
{
    public interface IActivityPubService
    {
        Task<Actor> GetUser(string objectId);
        Task<HttpStatusCode> PostDataAsync<T>(T data, string targetHost, string actorUrl, string inbox = null);
    }

    public class ActivityPubService : IActivityPubService
    {
        private readonly ICryptoService _cryptoService;

        #region Ctor
        public ActivityPubService(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }
        #endregion

        public async Task<Actor> GetUser(string objectId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                var result = await httpClient.GetAsync(objectId);
                var content = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Actor>(content);
            }
        }

        public async Task<HttpStatusCode> PostDataAsync<T>(T data, string targetHost, string actorUrl, string inbox = null)
        {
            var usedInbox = $"/inbox";
            if (!string.IsNullOrWhiteSpace(inbox))
                usedInbox = inbox;

            var json = JsonConvert.SerializeObject(data);

            var date = DateTime.UtcNow.ToUniversalTime();
            var httpDate = date.ToString("r");
            var signature = _cryptoService.SignAndGetSignatureHeader(date, actorUrl, targetHost, usedInbox);

            

            var client = new HttpClient();
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://{targetHost}/{usedInbox}"),
                Headers =
                {
                    {"Host", targetHost},
                    {"Date", httpDate},
                    {"Signature", signature}
                },
                Content = new StringContent(json, Encoding.UTF8, "application/ld+json")
            };

            var response = await client.SendAsync(httpRequestMessage);
            return response.StatusCode;
        }
    }
}