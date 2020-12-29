using BirdsiteLive.ActivityPub.Converters;
using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub.Models
{
    public class Followers
    {
        [JsonProperty("@context")]
        [JsonConverter(typeof(ContextArrayConverter))]
        public string context { get; set; } = "https://www.w3.org/ns/activitystreams";

        public string id { get; set; }
        public string type { get; set; } = "OrderedCollection";
    }
}