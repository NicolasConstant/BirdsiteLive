using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub.Models
{
    public class ActivityDelete : Activity
    {
        public string[] to { get; set; }
        [JsonProperty("object")]
        public object apObject { get; set; }
    }
}