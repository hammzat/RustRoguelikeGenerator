﻿using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using LZ4;
using System.Windows;
using UnityEngine;
using Newtonsoft.Json;

public class WorldSerialization
{
    public const uint CurrentVersion = 9;

    public uint Version { get; set; }

    public WorldData world = new WorldData();

    public WorldSerialization()
    {
        Version = CurrentVersion;
    }

    [ProtoContract]
    public class WorldData
    {
        [ProtoMember(1)] public uint size = 1000; // cause need use 1000!
        [ProtoMember(2)] public List<MapData> maps = new List<MapData>();
        [ProtoMember(3)] public List<PrefabData> prefabs = new List<PrefabData>();
        [ProtoMember(4)] public List<PathData> paths = new List<PathData>();
    }

    [ProtoContract]
    public class MapData
    {
        [ProtoMember(1)] public string name;
        [ProtoMember(2)] public byte[] data;
    }

    [Serializable]
    [ProtoContract]
    public class PrefabData
    {
        [JsonProperty("category")] 
        [ProtoMember(1)] public string category;
        [JsonProperty("id")] 
        [ProtoMember(2)] public uint id;
        [JsonProperty("position")] 
        [ProtoMember(3)] public VectorData position;
        [JsonProperty("rotation")] 
        [ProtoMember(4)] public VectorData rotation;
        [JsonProperty("scale")] 
        [ProtoMember(5)] public VectorData scale;


        public PrefabData() { }
        public PrefabData(string category, uint id, VectorData position, VectorData rotation, VectorData scale)
        {
            this.category = category;
            this.id = id;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    [Serializable]
    [ProtoContract]
    public class PathData
    {
        [ProtoMember(1)] public string name;
        [ProtoMember(2)] public bool spline;
        [ProtoMember(3)] public bool start;
        [ProtoMember(4)] public bool end;
        [ProtoMember(5)] public float width;
        [ProtoMember(6)] public float innerPadding;
        [ProtoMember(7)] public float outerPadding;
        [ProtoMember(8)] public float innerFade;
        [ProtoMember(9)] public float outerFade;
        [ProtoMember(10)] public float randomScale;
        [ProtoMember(11)] public float meshOffset;
        [ProtoMember(12)] public float terrainOffset;
        [ProtoMember(13)] public int splat;
        [ProtoMember(14)] public int topology;
        [ProtoMember(15)] public VectorData[] nodes;
    }

    [Serializable]
    [ProtoContract]
    public class VectorData
    {
        [JsonProperty("x")] [ProtoMember(1)] public float x;
        [JsonProperty("y")] [ProtoMember(2)] public float y;
        [JsonProperty("z")] [ProtoMember(3)] public float z;

        public VectorData()
        {
        }

        public VectorData(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator VectorData(Vector3 v)
        {
            return new VectorData(v.x, v.y, v.z);
        }

        public static implicit operator VectorData(Quaternion q)
        {
            return q.eulerAngles;
        }

        public static implicit operator Vector3(VectorData v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Quaternion(VectorData v)
        {
            return Quaternion.Euler(v);
        }

    }

    public MapData GetMap(string name)
    {
        for (int i = 0; i < world.maps.Count; i++)
            if (world.maps[i].name == name) return world.maps[i];
        return null;
    }

    public void AddMap(string name, byte[] data)
    {
        var map = new MapData();

        map.name = name;
        map.data = data;

        world.maps.Add(map);
    }

    public void Save(string fileName)
    {
        try
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(Version);

                    using (var compressionStream = new LZ4Stream(fileStream, LZ4StreamMode.Compress))
                        Serializer.Serialize(compressionStream, world);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public void Load(string fileName)
    {
        try
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    Version = binaryReader.ReadUInt32();
                   // if (Version != CurrentVersion)
                   //   MessageBox.Show("Map Version is: " + Version + " whilst Rust is on: " + CurrentVersion);

                    using (var compressionStream = new LZ4Stream(fileStream, LZ4StreamMode.Decompress))
                        world = Serializer.Deserialize<WorldData>(compressionStream);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    public WorldData GetWorldData(string fileName)
    {
        try
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    Version = binaryReader.ReadUInt32();
                   // if (Version != CurrentVersion)
                   //   MessageBox.Show("Map Version is: " + Version + " whilst Rust is on: " + CurrentVersion);

                    using (var compressionStream = new LZ4Stream(fileStream, LZ4StreamMode.Decompress))
                       return Serializer.Deserialize<WorldData>(compressionStream);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}