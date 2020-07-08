using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class ActivityAcceptUndoFollow : Activity
    {
        [JsonProperty("object")]
        public ActivityUndoFollow apObject { get; set; }
    }
}