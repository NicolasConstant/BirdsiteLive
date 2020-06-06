using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class ActivityUndo : Activity
    {
        [JsonProperty("object")]
        public Activity apObject { get; set; }
    }
}