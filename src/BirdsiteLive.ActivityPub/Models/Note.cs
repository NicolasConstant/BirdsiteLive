using System;
using System.Collections.Generic;
using BirdsiteLive.ActivityPub.Converters;
using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class Note
    {
        [JsonProperty("@context")]
        [JsonConverter(typeof(ContextArrayConverter))]
        public string[] context { get; set; } = new[] { "https://www.w3.org/ns/activitystreams" };

        public string id { get; set; }
        public string type { get; } = "Note";
        public string summary { get; set; }
        public string inReplyTo { get; set; }
        public string published { get; set; }
        public string url { get; set; }
        public string attributedTo { get; set; }
        public string[] to { get; set; }
        public string[] cc { get; set; }
        public bool sensitive { get; set; }
        //public string conversation { get; set; }
        public string content { get; set; }
        //public Dictionary<string,string> contentMap { get; set; }
        public string[] attachment { get; set; }
        public string[] tag { get; set; }
        //public Dictionary<string, string> replies;
    }
}