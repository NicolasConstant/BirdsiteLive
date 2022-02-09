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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<ActivityPubService> _logger;

        #region Ctor
        public ActivityPubService(ICryptoService cryptoService, InstanceSettings instanceSettings, IHttpClientFactory httpClientFactory, ILogger<ActivityPubService> logger)
        {
            _cryptoService = cryptoService;
            _instanceSettings = instanceSettings;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        #endregion

        public async Task<Actor> GetUser(string objectId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/activity+json");
            var result = await httpClient.GetAsync(objectId);

            if (result.StatusCode == HttpStatusCode.Gone)
                throw new FollowerIsGoneException();

            result.EnsureSuccessStatusCode();

            var content = await result.Content.ReadAsStringAsync();

            var actor = JsonConvert.DeserializeObject<Actor>(content);
            if (string.IsNullOrWhiteSpace(actor.url)) actor.url = objectId;
            return actor;
        }

        public async Task PostNewNoteActivity(Note note, string username, string noteId, string targetHost, string targetInbox)
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e, "Error sending {Username} post ({NoteId}) to {Host}{Inbox}", username, noteId, targetHost, targetInbox);
                throw;
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

            var digest = _cryptoService.ComputeSha256Hash(json);

            var signature = _cryptoService.SignAndGetSignatureHeader(date, actorUrl, targetHost, digest, usedInbox);

            var client = _httpClientFactory.CreateClient();
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