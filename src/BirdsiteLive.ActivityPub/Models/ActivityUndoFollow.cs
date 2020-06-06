using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class ActivityUndoFollow : Activity
    {
        [JsonProperty("object")]
        public ActivityFollow apObject { get; set; }
    }
}