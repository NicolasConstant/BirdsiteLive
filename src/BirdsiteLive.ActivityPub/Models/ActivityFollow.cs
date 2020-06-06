using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class ActivityFollow : Activity
    {
        [JsonProperty("object")]
        public string apObject { get; set; }
    }
}