using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.ActivityPub.Converters;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.Common.Settings;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;

namespace BirdsiteLive.Domain
{
    public interface IActivityPubService
    {
        Task<Actor> GetUser(string objectId);
        Task<HttpStatusCode> PostDataAsync<T>(T data, string targetHost, string actorUrl, string inbox = null);
        Task PostNewNoteActivity(Note note, string username, string noteId, string targetHost,
            string targetInbox);
    }

    public class ActivityPubService : IActivityPubService
    {
        private readonly InstanceSettings _instanceSettings;
        private readonly ICryptoService _cryptoService;

        #region Ctor
        public ActivityPubService(ICryptoService cryptoService, InstanceSettings instanceSettings)
        {
            _cryptoService = cryptoService;
            _instanceSettings = instanceSettings;
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

        public async Task PostNewNoteActivity(Note note, string username, string noteId, string targetHost, string targetInbox)
        {
            var actor = UrlFactory.GetActorUrl(_instanceSettings.Domain, username);
            var noteUri = UrlFactory.GetNoteUrl(_instanceSettings.Domain, username, noteId);

            var now = DateTime.UtcNow;
            var nowString = now.ToString("s") + "Z";
            
            var noteActivity = new ActivityCreateNote()
            {
                context = "https://www.w3.org/ns/activitystreams",
                id = $"{noteUri}/activity",
                type = "Create",
                actor = actor,
                published = nowString,

                to = note.to,
                cc = note.cc,
                apObject = note
            };

            await PostDataAsync(noteActivity, targetHost, actor, targetInbox);
        }

        public async Task<HttpStatusCode> PostDataAsync<T>(T data, string targetHost, string actorUrl, string inbox = null)
        {
            var usedInbox = $"/inbox";
            if (!string.IsNullOrWhiteSpace(inbox))
                usedInbox = inbox;

            var json = JsonConvert.SerializeObject(data);

            var date = DateTime.UtcNow.ToUniversalTime();
            var httpDate = date.ToString("r");

            var digest = _cryptoService.ComputeSha256Hash(json);

            var signature = _cryptoService.SignAndGetSignatureHeader(date, actorUrl, targetHost, digest, usedInbox);

            var client = new HttpClient(); //TODO: remove this from here
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://{targetHost}{usedInbox}"),
                Headers =
                {
                    {"Host", targetHost},
                    {"Date", httpDate},
                    {"Signature", signature},
                    {"Digest", $"SHA-256={digest}"}
                },
                Content = new StringContent(json, Encoding.UTF8, "application/ld+json")
            };

            var response = await client.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            return response.StatusCode;
        }
    }
}