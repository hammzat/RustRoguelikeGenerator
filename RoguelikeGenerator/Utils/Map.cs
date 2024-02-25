using Newtonsoft.Json;
using ProtoBuf;
using static WorldSerialization;

namespace RoguelikeGenerator.Utils
{
    public class Ways
    {
        public bool right;
        public bool left;
        public bool up;
        public bool down;
    }
    public class Map
    {
        public int size = 1000;
        public string checksum = string.Empty;

        public int meterSize = 1;
        public Ways ways = new Ways();

        [JsonIgnore] 
        public List<PrefabData> prefabs = new List<PrefabData>();

        public Map(int Size, List<PrefabData> prefabsData)
        {
            size = Size;
            prefabs = prefabsData;
        }
    }
}
