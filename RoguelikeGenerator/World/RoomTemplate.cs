using System.Globalization;
using System.Text;
using RoguelikeGenerator.Utils;
using static WorldSerialization;

namespace RoguelikeGenerator.World
{
    // Шаблон комнаты, загруженный из .map файла.
    // Все префабы хранятся в локальных координатах относительно центра пола.
    public class RoomTemplate
    {
        // Насколько близко к краю комнаты должен стоять префаб, чтобы считаться частью стены.
        private const float WallEdgeTolerance = 0.5f;

        public string Name { get; }
        public float CellSize { get; }
        public float FloorY { get; }
        public Direction? DoorSide { get; }
        public IReadOnlyList<PrefabData> LocalPrefabs { get; }

        private RoomTemplate(string name, float cellSize, float floorY, Direction? doorSide, List<PrefabData> localPrefabs)
        {
            Name = name;
            CellSize = cellSize;
            FloorY = floorY;
            DoorSide = doorSide;
            LocalPrefabs = localPrefabs;
        }

        public static RoomTemplate Load(string path)
        {
            var world = new WorldSerialization();
            world.Load(path);
            var prefabs = world.world.prefabs;
            if (prefabs.Count == 0)
                throw new InvalidDataException($"В {path} нет ни одного префаба");

            // Пол = самый широкий по XZ префаб, а среди одинаковых — самый нижний.
            var floor = prefabs
                .OrderByDescending(p => p.scale.x * p.scale.z)
                .ThenBy(p => p.position.y)
                .First();

            float cellSize = Math.Max(floor.scale.x, floor.scale.z);
            var origin = floor.position;

            var local = prefabs.Select(p => new PrefabData(
                p.category,
                p.id,
                new VectorData(p.position.x - origin.x, p.position.y - origin.y, p.position.z - origin.z),
                p.rotation,
                p.scale)).ToList();

            var doorSide = DetectDoorSide(local, cellSize);
            return new RoomTemplate(Path.GetFileNameWithoutExtension(path), cellSize, origin.y, doorSide, local);
        }

        // Сторона, к которой прижат префаб (если он стоит у края комнаты), иначе null.
        public Direction? SideOf(VectorData localPos)
        {
            float edge = CellSize / 2 - WallEdgeTolerance;
            if (localPos.z >= edge) return Direction.North;
            if (localPos.z <= -edge) return Direction.South;
            if (localPos.x >= edge) return Direction.East;
            if (localPos.x <= -edge) return Direction.West;
            return null;
        }

        // Дверь = сторона, стены которой у пола закрывают не всю ширину комнаты.
        private static Direction? DetectDoorSide(List<PrefabData> local, float cellSize)
        {
            var coverage = DirectionExt.All.ToDictionary(d => d, _ => 0f);
            float edge = cellSize / 2 - WallEdgeTolerance;

            foreach (var p in local)
            {
                // Только то, что стоит на полу (перемычка над дверью не считается).
                float bottom = p.position.y - p.scale.y / 2;
                if (bottom > WallEdgeTolerance) continue;
                // Только вертикальные "плиты": высокие и тонкие.
                if (p.scale.y < 1f || Math.Min(p.scale.x, p.scale.z) > 0.5f) continue;

                Direction? side =
                    p.position.z >= edge ? Direction.North :
                    p.position.z <= -edge ? Direction.South :
                    p.position.x >= edge ? Direction.East :
                    p.position.x <= -edge ? Direction.West : null;
                if (side == null) continue;

                // Для стен, повёрнутых на 90/270, длина вдоль стороны тоже scale.x.
                coverage[side.Value] += p.scale.x;
            }

            var candidates = coverage
                .Where(kv => kv.Value > 0 && kv.Value < cellSize * 0.9f)
                .Select(kv => kv.Key)
                .ToList();
            return candidates.Count == 1 ? candidates[0] : null;
        }

        public string Describe()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Шаблон: {Name}");
            sb.AppendLine($"Размер клетки: {CellSize.ToString(CultureInfo.InvariantCulture)} м, высота пола: {FloorY.ToString(CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Дверь: {(DoorSide?.ToString() ?? "не найдена")}");
            sb.AppendLine($"Префабов: {LocalPrefabs.Count}");
            foreach (var p in LocalPrefabs)
            {
                var side = SideOf(p.position);
                sb.AppendLine($"  {p.id,-11} pos {p.position.VectorData2String(),-30} rot {p.rotation.VectorData2String(),-12} scale {p.scale.VectorData2String(),-12} {(side?.ToString() ?? "")}");
            }
            return sb.ToString();
        }
    }
}
