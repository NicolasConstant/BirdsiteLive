using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub.Converters
{
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