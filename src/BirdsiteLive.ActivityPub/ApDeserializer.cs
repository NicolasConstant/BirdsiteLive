using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class ApDeserializer
    {
        public static Activity ProcessActivity(string json)
        {
            var activity = JsonConvert.DeserializeObject<Activity>(json);
            switch (activity.type)
            {
                case "Follow":
                    return JsonConvert.DeserializeObject<ActivityFollow>(json);
                case "Undo":
                    var a = JsonConvert.DeserializeObject<ActivityUndo>(json);
                    if(a.apObject.type == "Follow")
                        return JsonConvert.DeserializeObject<ActivityUndoFollow>(json);
                    break;
            }

            return null;
        }

        private class Ac : Activity
        {
            [JsonProperty("object")]
            public Activity apObject { get; set; }
        }
    }
}