using System;
using System.Text.Json.Serialization;

namespace BirdsiteLive.ActivityPub
{
    public class Actor
    {
        [JsonPropertyName("@context")]
        public string[] context { get; set; } = new[] {"https://www.w3.org/ns/activitystreams", "https://w3id.org/security/v1"};
        public string id { get; set; }
        public string type { get; set; }
        public string preferredUsername { get; set; }
        public string name { get; set; }
        public string summary { get; set; }
        public string url { get; set; }
        public string inbox { get; set; }
        public PublicKey publicKey { get; set; }
        public Image icon { get; set; }
        public Image image { get; set; }
    }
}
