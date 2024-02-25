using RoguelikeGenerator.Utils;
using RoguelikeGenerator.World;
using System.Runtime.CompilerServices;
using System;
using System.IO;
using System.Dynamic;
using static WorldSerialization;
using ProtoBuf;

namespace RoguelikeGenerator 
{
    internal class Program
    {
        private static void Print(object message)
        {
            Console.Write(message);
        }
        private static void PrintErr(object message)
        {
            Console.WriteLine("[-] " + message);
        }
        private static void PrintGood(object message)
        {
            Console.WriteLine("[+] " + message);
        }
        private static void Checks()
        {
            if (!Directory.Exists("maps"))
            {
                PrintErr("Папка не найдена, создаем...");
                Directory.CreateDirectory("maps");
            }
            if (!Path.Exists("maps/*.map"))
            {
                PrintErr("Не найдены файлы карт, загружаем дефолтные...");
                Downloader.LoadDefaultMaps();
                PrintGood("Скачивание завершено!");
            }
        }
        private static void ProceedMapFiles()
        {
            var files = Directory.EnumerateFiles("maps", "*.map");
            if (files.Count() == 0) return;
            
            foreach (string path in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);

                Print($"Обработка файла {fileName}...\n");

                MapManager mapManager = new MapManager(path);
                mapManager.ProceedMap();
                Map map = mapManager.GetMap();

                if (!Path.Exists($"maps/{fileName}.json") || !MD5Hash.Equals(map.checksum, path)) {
                    PrintGood($"Создание конфига для {fileName}...\n");
                    map.checksum = MD5Hash.Calculate(path);

                    Print("Введите размер пола комнаты без учёта Scale:");
                    int meterSize;
                    if (!int.TryParse(Console.ReadLine(), out meterSize))
                    {
                        meterSize = 1;
                        PrintGood("Поставлено дефолтное - 1 метр!");
                    }
                    map.meterSize = meterSize;

                    Config.WriteJson(map, $"maps/{fileName}.json");

                }
            }
        }

        private static WorldSerialization _worldSerialization = new WorldSerialization();
        private static void Main(string[] args)
        {
            Console.Title = "RustMap Roguelike Generator";
            //int gridSize = 6;

            Print("RustMap Roguelike Generator | by aristocratos " +
                "\nДля использования генератора вам необходимы файлы .map в /maps/ и json файл с настройками.\n\n\n");
            
            Checks();
            //ProceedMapFiles();
            Thread.Sleep(500);

            //Print("Введите сетку карты (16x16, 10x10), только 1 число:");
            //if (!int.TryParse(Console.ReadLine(), out gridSize))
            //{
            //    PrintErr("Нужно ввести только число!");
            //}
            var files = Directory.GetFiles("maps", "*.map");
            MapManager mapManager = new MapManager(files[1]); // обработка карты
            mapManager.ProceedMap(); // получение данных карты, размер, префабы и тд
            Map map = mapManager.GetMap();
            _worldSerialization.Load(files[1]);// тут я заебался и напрямую брал файлы из папки
            PrintGood(files[0]);
            PrintGood(files[1]);
            _worldSerialization.world.prefabs = map.prefabs;
            var prefabs = MapGenerator.CreatePrefabFromMap(map); // ну тут да
            foreach (var prefab in prefabs)
            {
                _worldSerialization.world.prefabs.Add(prefab);
                _worldSerialization.Save("final.map"); // сохранение карты
            }
        }
    }
}