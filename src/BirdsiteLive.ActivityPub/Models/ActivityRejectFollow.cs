using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class ActivityRejectFollow : Activity
    {
        [JsonProperty("object")]
        public ActivityFollow apObject { get; set; }
    }
}