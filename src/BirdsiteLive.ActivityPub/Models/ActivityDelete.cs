using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub.Models
{
    public class ActivityDelete : Activity
    {
        [JsonProperty("object")]
        public object apObject { get; set; }
    }
}