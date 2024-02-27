using ProtoBuf.ServiceModel;
using RoguelikeGenerator.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ExceptionServices;
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
        public static List<PrefabData> CreatePrefabFromMap(string pathToMap, int col = 1, int row = 1, string category = "generatedbyRoguelike")
        {
            MapManager mapManager = new MapManager(pathToMap);
            mapManager.ProceedMap();
            Map map = mapManager.GetMap();
            map.meterSize = 1;
            return CreatePrefabFromMap(map, col, row, category);
        }

        //public static List<PrefabData> oldCreatePrefabFromMap(Map map, int col = 1, int row = 1, string category = "generatedbyRoguelike")
        //{
        //    List<PrefabData> createdPrefabs = new();
        //    bool first = true;

        //    Console.WriteLine($"[GENERATOR] Proceeding Room at ({col};{row})");
        //    foreach (var prefab in map.prefabs)
        //    {
        //        createdPrefabs.Add(
        //            CreatePrefab(
        //                prefab.id, 
        //                CalculatePosition(prefab.position, prefab.scale, map.meterSize, col, row, s), 
        //                prefab.rotation, 
        //                prefab.scale, 
        //                category
        //        ));
        //    }
        //    return createdPrefabs;
        //}
        //private static VectorData oldCalculatePosition(VectorData globalpos, VectorData scale, int meterSize, int col = 1, int row = 1)
        //{
        //    VectorData nextSize; // следующая позиция для генерации
        //    VectorData startPos = new VectorData(0, 100, 0); // стартовая позиция для генерации
        //    VectorData localpos = CalculateLocalPos(new VectorData(0, 100, 0), globalpos);
        //    float Increment = meterSize * scale.x; Console.WriteLine("Scale: " + scale.VectorData2String());
        //    //float zIncrement = meterSize * scale.z;

        //    float nextX = startPos.x + localpos.x + (col - 1) * Increment;      // Работает
        //    float nextY = startPos.y + localpos.y;                              // Работает
        //    float nextZ = startPos.z + localpos.z + (row - 1) * Increment;      // Работает

        //    nextSize = new VectorData(nextX, nextY, nextZ);

        //    Console.WriteLine($"[G] -> Placed at pos ({nextX}, {nextY}, {nextZ})\n\n");
        //    return nextSize;
        //}


        private static VectorData CalculateLocalPos(VectorData placePos, VectorData globalpos)
        {
            VectorData localpos = new VectorData(globalpos.x - placePos.x, globalpos.y - placePos.y, globalpos.z - placePos.z);
            Console.WriteLine($"[LCL] -> X: {localpos.x}, Y: {localpos.y}, Z: {localpos.z}");
            return localpos;
        }
        private static VectorData CalculateGlobalPosition(VectorData startPos, int col, int row, VectorData scale, int meterSize = 1)
        {
            VectorData nextSize; 
            float Increment = meterSize * scale.x;

            float nextX = startPos.x + (col - 1) * Increment;      // Работает
            float nextY = startPos.y;                              // Работает
            float nextZ = startPos.z + (row - 1) * Increment;      // Работает

            nextSize = new VectorData(nextX, nextY, nextZ);

            Console.WriteLine($"[G] -> Global Position: ({nextX}, {nextY}, {nextZ})\n\n");
            return nextSize;
        }

        public static List<PrefabData> CreatePrefabFromMap(Map map, int col = 1, int row = 1, string category = "generatedbyRoguelike")
        {
            List<PrefabData> createdPrefabs = new();
            bool first = true;
            VectorData position = new VectorData(20, 100, 0);
            VectorData startPos = new (0, 100, 0);
            Console.WriteLine($"\n\n[GENERATOR] Proceeding Room at ({col};{row})");
            foreach (var prefab in map.prefabs)
            {
                if (first)
                {
                    first = false;
                    position = CalculateGlobalPosition(startPos, col, row, prefab.scale, map.meterSize);
                }
                createdPrefabs.Add(
                    CreatePrefab(
                        prefab.id,
                        Calculate(position, prefab.position, prefab.scale, map.meterSize, col, row),
                        prefab.rotation,
                        prefab.scale,
                        category
                ));
            }
            return createdPrefabs;
        }

        private static VectorData Calculate(VectorData globalpos, VectorData position, VectorData scale, int meterSize, int col = 1, int row = 1, int s = 0)
        {
            VectorData nextSize; // следующая позиция для генерации
            VectorData localpos = CalculateLocalPos(new(0, 100, 0), position);
            float Increment = meterSize * scale.x; Console.WriteLine("Scale: " + scale.VectorData2String());
            //float zIncrement = meterSize * scale.z;

            float nextX = globalpos.x + localpos.x;
            float nextY = globalpos.y + localpos.y;
            float nextZ = globalpos.z + localpos.z;

            nextSize = new VectorData(nextX, nextY, nextZ);

            Console.WriteLine($"[G] -> Placed at pos ({nextX}, {nextY}, {nextZ})\n\n");
            return nextSize;
        }
    }
}
