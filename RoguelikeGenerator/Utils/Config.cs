using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeGenerator.Utils
{
    public static class Config
    {
        // TODO: починить ebuchie конфиги для карт
        public static Map FromJson<Map>(string jsonString)
        {
            JsonSerializer serializer = new JsonSerializer();
            return FromJson<Map>(serializer, jsonString);
        }
        public static Map FromJson<Map>(JsonSerializer serializer, string jsonString)
        {
            Map obj;
            using (var reader = new StringReader(jsonString))
            using (var jsonReader = new JsonTextReader(reader))
            {
                obj = serializer.Deserialize<Map>(jsonReader);
            }
            return obj;
        }
        public static string ToJson(Map payload)
        {
            JsonSerializer serializer = new JsonSerializer();
            return ToJson(serializer, payload);
        }
        public static string ToJson(JsonSerializer serializer, Map payload)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                serializer.Serialize(stringWriter, payload);
            }
            var jsonStr = stringBuilder.ToString();
            return jsonStr;
        }

        public static void WriteJson(Map payload, string path)
        {
            File.WriteAllText(path, ToJson(payload));
        }
    }
}
