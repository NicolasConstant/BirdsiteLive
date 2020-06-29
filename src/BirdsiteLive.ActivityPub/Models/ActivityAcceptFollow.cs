using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class ActivityAcceptFollow : Activity
    {
        [JsonProperty("object")]
        public ActivityFollow apObject { get; set; }
    }
}