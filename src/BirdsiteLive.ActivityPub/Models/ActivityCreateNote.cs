using System;
using BirdsiteLive.ActivityPub.Models;
using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class ActivityCreateNote : Activity
    {
        public string published { get; set; }
        public string[] to { get; set; }
        public string[] cc { get; set; }

        [JsonProperty("object")]
        public Note apObject { get; set; }
    }
}