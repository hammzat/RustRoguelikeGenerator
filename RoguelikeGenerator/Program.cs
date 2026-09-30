using RoguelikeGenerator.Utils;
using RoguelikeGenerator.World;

namespace RoguelikeGenerator
{
    internal class Program
    {
        private const string MapsDir = "maps";
        private const string BaseMap = "maps/_base.map";
        private const string RoomPattern = "map_room*.map";
        private const string OutputMap = "finalresult.map";

        private static void PrintErr(object message) => Console.WriteLine("[-] " + message);
        private static void PrintGood(object message) => Console.WriteLine("[+] " + message);

        private static int Main(string[] args)
        {
            if (!Console.IsOutputRedirected) Console.Title = "RustMap Roguelike Generator";
            Console.WriteLine("RustMap Roguelike Generator | by aristocratos\n");

            if (args.Length == 2 && args[0] == "--info")
            {
                Console.WriteLine(RoomTemplate.Load(args[1]).Describe());
                return 0;
            }

            try
            {
                EnsureMaps();
            }
            catch (Exception e)
            {
                PrintErr($"Не удалось скачать карты: {e.Message}");
                return 1;
            }

            var templates = LoadTemplates();
            if (templates.Count == 0)
            {
                PrintErr($"В папке {MapsDir} нет шаблонов комнат ({RoomPattern})");
                return 1;
            }

            // Аргументы: [сетка] [кол-во комнат] [seed]. Чего нет — спрашиваем.
            int gridSize = ArgOrAsk(args, 0, "Размер сетки (одно число, например 10 для 10x10)", 3, min: 1, max: 100);
            int maxRooms = gridSize * gridSize;
            int roomCount = ArgOrAsk(args, 1, $"Сколько комнат (1..{maxRooms})", Math.Min(5, maxRooms), min: 1, max: maxRooms);
            int seed = ArgOrAsk(args, 2, "Seed (пусто = случайный)", Random.Shared.Next(), min: int.MinValue, max: int.MaxValue);

            var rng = new Random(seed);
            var layout = DungeonLayout.Generate(gridSize, roomCount, rng);
            Console.WriteLine($"\nSeed: {seed}\n{layout.Render()}\n");

            var world = new WorldSerialization();
            world.Load(BaseMap);
            var prefabs = MapGenerator.Build(layout, templates, rng);
            world.world.prefabs.AddRange(prefabs);
            world.Save(OutputMap);

            PrintGood($"Сгенерировано: {layout.Rooms.Count} комнат, {prefabs.Count} префабов -> {OutputMap}");

            if (!Console.IsInputRedirected && args.Length == 0)
                Console.ReadKey();
            return 0;
        }

        private static void EnsureMaps()
        {
            Directory.CreateDirectory(MapsDir);
            if (!File.Exists(BaseMap) || !Directory.EnumerateFiles(MapsDir, RoomPattern).Any())
            {
                PrintErr("Не найдены файлы карт, загружаем дефолтные...");
                Downloader.LoadDefaultMaps(MapsDir);
                PrintGood("Скачивание завершено!\n");
            }
        }

        private static List<RoomTemplate> LoadTemplates()
        {
            var templates = new List<RoomTemplate>();
            foreach (var path in Directory.EnumerateFiles(MapsDir, RoomPattern).OrderBy(p => p))
            {
                try
                {
                    var template = RoomTemplate.Load(path);
                    templates.Add(template);
                    PrintGood($"Шаблон {template.Name}: {template.LocalPrefabs.Count} префабов, клетка {template.CellSize} м, дверь: {template.DoorSide?.ToString() ?? "нет"}");
                }
                catch (Exception e)
                {
                    PrintErr($"Пропускаем {path}: {e.Message}");
                }
            }
            return templates;
        }

        private static int ArgOrAsk(string[] args, int index, string prompt, int fallback, int min, int max)
        {
            if (index < args.Length)
            {
                if (int.TryParse(args[index], out int fromArg) && fromArg >= min && fromArg <= max)
                    return fromArg;
                PrintErr($"Аргумент \"{args[index]}\" не подходит, используем {fallback}");
                return fallback;
            }

            Console.Write($"{prompt} [{fallback}]: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return fallback;
            if (int.TryParse(input, out int value) && value >= min && value <= max)
                return value;

            PrintErr($"Нужно число от {min} до {max}, используем {fallback}");
            return fallback;
        }
    }
}
