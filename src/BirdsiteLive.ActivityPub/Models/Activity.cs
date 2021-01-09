using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class Activity
    {
        [JsonProperty("@context")]
        public object context { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string actor { get; set; }

        //[JsonProperty("object")]
        //public string apObject { get; set; }
    }
}