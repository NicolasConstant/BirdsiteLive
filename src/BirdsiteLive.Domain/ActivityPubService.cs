using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
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
        Task<HttpStatusCode> PostNewNoteActivity(Note note, string username, string noteId, string targetHost,
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

        public async Task<HttpStatusCode> PostNewNoteActivity(Note note, string username, string noteId, string targetHost, string targetInbox)
        {
            //var username = "gra";
            var actor = $"https://{_instanceSettings.Domain}/users/{username}";
            //var targetHost = "mastodon.technology";
            //var target = $"{targetHost}/users/testtest";
            //var inbox = $"/users/testtest/inbox";

            //var noteGuid = Guid.NewGuid();
            var noteUri = $"https://{_instanceSettings.Domain}/users/{username}/statuses/{noteId}";
            
            //var noteUrl = $"https://{_instanceSettings.Domain}/@{username}/{noteId}";
            //var to = $"{actor}/followers";
            //var apPublic = "https://www.w3.org/ns/activitystreams#Public";

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
                //apObject = new Note()
                //{
                //    id = noteUri,
                //    summary = null,
                //    inReplyTo = null,
                //    published = nowString,
                //    url = noteUrl,
                //    attributedTo = actor,
                //    to = new[] { to },
                //    //cc = new [] { apPublic },
                //    sensitive = false,
                //    content = "<p>Woooot</p>",
                //    attachment = new string[0],
                //    tag = new string[0]
                //}
            };

            //TODO Remove this
            if (targetInbox.Contains(targetHost))
                targetInbox = targetInbox.Split(new []{ targetHost }, StringSplitOptions.RemoveEmptyEntries).Last();

            return await PostDataAsync(noteActivity, targetHost, actor, targetInbox);
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