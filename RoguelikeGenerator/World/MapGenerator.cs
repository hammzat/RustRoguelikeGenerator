using RoguelikeGenerator.Utils;
using static WorldSerialization;

namespace RoguelikeGenerator.World
{
    public static class MapGenerator
    {
        public const string DefaultCategory = "generatedbyRoguelike";

        // Превращает раскладку в список префабов для карты.
        // Каждая комната — случайный шаблон, повернутый дверью к комнате-родителю;
        // стены на сторонах с проходом убираются (кроме стены с дверным проёмом).
        public static List<PrefabData> Build(DungeonLayout layout, IReadOnlyList<RoomTemplate> templates, Random rng, string category = DefaultCategory)
        {
            if (templates.Count == 0) throw new ArgumentException("Нужен хотя бы один шаблон комнаты", nameof(templates));

            float cell = templates.Max(t => t.CellSize);
            float offset = (layout.GridSize - 1) / 2f * cell;
            var result = new List<PrefabData>();

            foreach (var room in layout.Rooms)
            {
                var template = templates[rng.Next(templates.Count)];
                int turns = ChooseTurns(template, room, rng);
                Direction? door = template.DoorSide?.Rotate(turns);

                var center = new VectorData(room.Col * cell - offset, template.FloorY, room.Row * cell - offset);

                foreach (var prefab in template.LocalPrefabs)
                {
                    var local = RotateY(prefab.position, turns);
                    var side = template.SideOf(local);
                    if (side != null && side != door && room.Ways.Contains(side.Value))
                        continue; // тут проход в соседнюю комнату

                    result.Add(new PrefabData(
                        category,
                        prefab.id,
                        new VectorData(center.x + local.x, center.y + local.y, center.z + local.z),
                        new VectorData(prefab.rotation.x, (prefab.rotation.y + turns * 90f) % 360f, prefab.rotation.z),
                        new VectorData(prefab.scale.x, prefab.scale.y, prefab.scale.z)));
                }
            }

            return result;
        }

        private static int ChooseTurns(RoomTemplate template, Room room, Random rng)
        {
            if (template.DoorSide == null)
                return rng.Next(4);

            Direction target = room.Parent
                ?? (room.Ways.Count > 0 ? room.Ways.ElementAt(rng.Next(room.Ways.Count)) : template.DoorSide.Value);
            return template.DoorSide.Value.TurnsTo(target);
        }

        // Поворот вокруг оси Y на turns * 90° по часовой (как в Unity): North -> East -> South -> West.
        private static VectorData RotateY(VectorData v, int turns) => (turns % 4) switch
        {
            1 => new VectorData(v.z, v.y, -v.x),
            2 => new VectorData(-v.x, v.y, -v.z),
            3 => new VectorData(-v.z, v.y, v.x),
            _ => new VectorData(v.x, v.y, v.z)
        };
    }
}
