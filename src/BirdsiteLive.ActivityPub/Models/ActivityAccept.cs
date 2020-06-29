using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class ActivityAccept : Activity
    {
        [JsonProperty("object")]
        public object apObject { get; set; }
    }
}