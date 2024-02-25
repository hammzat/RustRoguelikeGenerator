using ProtoBuf.ServiceModel;
using RoguelikeGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static WorldSerialization;

namespace RoguelikeGenerator.World
{
    internal class MapGenerator
    {
        private static PrefabData CreatePrefab(uint PrefabID, VectorData posistion, VectorData rotation, VectorData scale = null, string category = ":\\test black:1:")
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

        public static List<PrefabData> CreatePrefabFromMap(Map map, int toGen = 90, string category = "generatedbyRoguelike")
        {
            List<PrefabData> createdPrefabs = new();
            for (int i = 0; i <= toGen; i++)
            {
                int s = 0;
                foreach (var prefab in map.prefabs)
                {
                    s++;
                    createdPrefabs.Add(
                        CreatePrefab(
                            prefab.id, 
                            CalculatePosition(prefab.position, prefab.scale, map.meterSize, i, s), 
                            prefab.rotation, 
                            prefab.scale, 
                            category
                    ));
                }
                Console.WriteLine("Cycle " + i);
            }
            return createdPrefabs;
        }


        private static VectorData CalculatePosition(VectorData pos, VectorData scale, int meterSize, int i, int s)
        {
            VectorData nextSize; // следующая позиция для генерации
            VectorData startPos = new VectorData(10, 100, 10); // стартовая позиция для генерации
            if (s == 1 || s == 2) // первые 2 элемента в иерархии префабов будут потолком и полом (костыль)
            {
                nextSize = new VectorData(startPos.x + meterSize * i * scale.x, pos.y, startPos.z);
                return nextSize;
            }
            nextSize = new VectorData(startPos.x + pos.x + meterSize * i * scale.x, pos.y, startPos.z + pos.z);
            return nextSize;
        }
    }
}
