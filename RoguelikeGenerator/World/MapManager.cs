using static WorldSerialization;
using RoguelikeGenerator.Utils;
using System.Text;

namespace RoguelikeGenerator.World
{
    internal class MapManager (string path)
    {
        private WorldSerialization _worldSerialization = new WorldSerialization();
        private string _path = path;
        private Random _rnd = new Random();
        private int _size = 0;

        public string MapFile { get; set; }
        public Map GetMap() => new Map(_size, _worldSerialization.world.prefabs);

        public void ProceedMap()
        {
            LoadWorldData();
            GetInfoMap();
            //ProcessProtection();
            //ProcessPrefabs();
            //ProcessPumpJackOverflow();
            //ProcessMapDataOverflow();
            //ProcessSpamPrefabs();
            //ShufflePrefabList();
            //PatchAndSavePluginFile();
        }

        private void GetInfoMap() // чисто для себя сделал чтобы парсить все префабы на карте
        {
            var info = _worldSerialization.world;
            var mapinfo = $"SIZE: {info.size} \nPREFABS ({info.prefabs.Count}):\n";
            foreach (var prefab in info.prefabs)
            {
                mapinfo += $"------- new prefab ---------------------\n";
                mapinfo += $"PREFAB UID: {prefab.id}\n";
                mapinfo += $"PREFAB Pos: {prefab.position.VectorData2String()}\n";
                mapinfo += $"PREFAB Rot: {prefab.rotation.VectorData2String()}\n";
                mapinfo += $"PREFAB Scale: {prefab.scale.VectorData2String()}\n";
                mapinfo += "-------finished for prefab---------------\n";
            }
            mapinfo += "\n\n\n\n\n\n";
            foreach (var map in info.maps)
            {
                mapinfo += $"------- new map ---------------------\n";
                mapinfo += $"PREFAB Name: {map.name}\n";
                //mapinfo += $"PREFAB Data: \n{}\n";
                mapinfo += "-------finished for map---------------\n";
            }
            File.WriteAllText("mapinfo.txt", mapinfo);

            //_worldSerialization.world.prefabs.Add(CreatePrefab(1266085737, new VectorData(0, 200, 0), new VectorData(0, 0, 0), new VectorData(4, 0.2f, 4), "generatedbyRogulike"));
            //_worldSerialization.Save("sussymap.map");
        }

        private void LoadWorldData()
        {
            _worldSerialization.Load(_path);
            _size = (int)_worldSerialization.world.size;
        }

        private void ProcessPrefabs()
        {
            //for (int i = _worldSerialization.world.prefabs.Count - 1; i >= 0; i--)
            //{
            //    if (_prefabEntity.IsEntity(_worldSerialization.world.prefabs[i].id))
            //    {
            //        PrefabData p = _worldSerialization.world.prefabs[i];
            //        _addPrefabs.Add(new PA().New(p.id, p.category, p.position, p.rotation, p.scale));
            //        _worldSerialization.world.prefabs.RemoveAt(i);
            //        continue;
            //    }

            //    if (_worldSerialization.world.prefabs[i].id != 1724395471)
            //    {
            //        _worldSerialization.world.prefabs[i].category = $":\\\\test black:{_rnd.Next(0, Math.Min(_worldSerialization.world.prefabs.Count, 40))}:";
            //    }
            //}
        }


        private PrefabData CreatePrefab(uint PrefabID, VectorData posistion, VectorData rotation, VectorData scale = null, string category = ":\\test black:1:")
        {
            var prefab = new PrefabData()
            {
                category = category,
                id = PrefabID,
                position = posistion,
                rotation = rotation,
                scale = scale == null ? new VectorData(1, 1, 1) : scale
            };
            return prefab;
        }
    }
}
