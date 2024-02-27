using RoguelikeGenerator.Utils;
using RoguelikeGenerator.World;
using UnityEngine;
using static WorldSerialization;

namespace RoguelikeGenerator 
{
    internal class Program
    {
        private static void PrintErr(object message) => Console.WriteLine("[-] " + message);
        private static void PrintGood(object message) => Console.WriteLine("[+] " + message);
        private static void Checks()
        {
            if (!Directory.Exists("maps"))
            {
                PrintErr("Папка не найдена, создаем...");
                Directory.CreateDirectory("maps");
            }
            if (!Path.Exists("maps/_base.map"))
            {
                PrintErr("Не найдены файлы карт, загружаем дефолтные...");
                Downloader.LoadDefaultMaps();
                PrintGood("Скачивание завершено!\n\n");
                Thread.Sleep(1000);
            }
        }
        private static void ProceedMapFiles()
        {
            var files = Directory.EnumerateFiles("maps", "*.map");
            if (files.Count() == 0) return;
            
            foreach (string path in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);

                Console.WriteLine($"Обработка файла {fileName}...\n");

                MapManager mapManager = new MapManager(path);
                mapManager.ProceedMap();
                Map map = mapManager.GetMap();

                if (!Path.Exists($"maps/{fileName}.json")) {
                    PrintGood($"Создание конфига для {fileName}...\n");

                    Console.WriteLine("Введите размер пола комнаты без учёта Scale:");
                    int meterSize = 0;
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
            int gridSize = 3;

            Console.WriteLine("RustMap Roguelike Generator | by aristocratos " +
                "\nДля использования генератора вам необходимы файлы .map в /maps/ и json файл с настройками.\n\n\n");
            
            Checks();
            //ProceedMapFiles();
            Thread.Sleep(500);

            Console.WriteLine("Введите сетку карты (16x16, 10x10), только 1 число: ");
            if (!int.TryParse(Console.ReadLine(), out gridSize))
            {
                PrintErr("Вы не ввели число, поставлено дефолтная сетка 3x3!");
            }


            int toGen;
            _worldSerialization.Load("maps/_base.map");

            Console.WriteLine($"Сколько комнат: (< {gridSize * gridSize})");
            if (!int.TryParse(Console.ReadLine(), out toGen))
            {
                PrintErr("Вы не ввели число..."); return;
            }
            if (gridSize * gridSize == toGen)
            {
                PrintErr("Слишком много комнат для генерации"); return;
            }
            int col, row;
            bool[,] table = new bool[gridSize, gridSize];


            for (int i = 0; i < toGen; i++)
            {
                System.Random rng = new();
                col = rng.Next(1, gridSize);
                row = rng.Next(1, gridSize);
                bool tablePlace = table[col, row];
                if (tablePlace) { i--; continue; }
                table[col, row] = true;
                _worldSerialization.world.prefabs.AddRange(MapGenerator.CreatePrefabFromMap("maps/map_room_ext.map", col, row));

            }

            _worldSerialization.Save("finalresult.map");
            PrintGood("Сгенерировано!");
            Console.ReadKey();



            // тест табличного плейса комнат
            //while (true)
            //{
            //    _try++;
            //    Console.WriteLine($"[{_try}] Номер строки: (x =< {gridSize})");
            //    if (!int.TryParse(Console.ReadLine(), out row))
            //    {
            //        PrintErr("Вы не ввели число, окончание генерации...");
            //        break;
            //    }
            //    Console.WriteLine($"[{_try}] Номер колонки: (y =< {gridSize})");
            //    if (!int.TryParse(Console.ReadLine(), out col))
            //    {
            //        PrintErr("Вы не ввели число, окончание генерации...");
            //        break;
            //    }

            //    if (row > gridSize || col > gridSize) {
            //        PrintErr("Вы ввели число больше сетки, повторите ещё раз...");
            //        _try--;
            //        goto InfoGenerator;
            //    }


            //    foreach (PrefabData prefab in MapGenerator.CreatePrefabFromMap("maps/map_room_ext.map", col, row))
            //    {
            //        _worldSerialization.world.prefabs.Add(prefab);
            //    }
            //    Thread.Sleep(1000);
            //    //Console.Clear();
            //}
        }
    }
}