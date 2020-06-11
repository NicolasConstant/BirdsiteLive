using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace BirdsiteLive.ActivityPub
{
    public class Actor
    {
        //[JsonPropertyName("@context")]
        [JsonProperty("@context")]
        [JsonConverter(typeof(ContextArrayConverter))]
        public string[] context { get; set; } = new[] { "https://www.w3.org/ns/activitystreams", "https://w3id.org/security/v1" };
        public string id { get; set; }
        public string type { get; set; }
        public string preferredUsername { get; set; }
        public string name { get; set; }
        public string summary { get; set; }
        public string url { get; set; }
        public string inbox { get; set; }
        public PublicKey publicKey { get; set; }
        public Image icon { get; set; }
        public Image image { get; set; }
    }

    public class ContextArrayConverter : JsonConverter
    {
        public override bool CanWrite { get { return false; } }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = new List<string>();

            var list = serializer.Deserialize<List<object>>(reader);
            foreach (var l in list)
            {
                if (l is string s)
                    result.Add(s);
                else
                {
                    var str = JsonConvert.SerializeObject(l);
                    result.Add(str);
                }
            }

            return result.ToArray();
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
