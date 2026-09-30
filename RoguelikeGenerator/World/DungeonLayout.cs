using RoguelikeGenerator.Utils;

namespace RoguelikeGenerator.World
{
    public class Room
    {
        public int Col { get; }
        public int Row { get; }
        // Кто открыл эту комнату (null у стартовой).
        public Direction? Parent { get; set; }
        // Стороны, через которые есть проход в соседние комнаты.
        public HashSet<Direction> Ways { get; } = new();

        public Room(int col, int row)
        {
            Col = col;
            Row = row;
        }
    }

    // Раскладка данжа на сетке: какие клетки заняты и какие из них соединены.
    // Все комнаты гарантированно связаны (остовное дерево + немного петель).
    public class DungeonLayout
    {
        public int GridSize { get; }
        public IReadOnlyList<Room> Rooms => _rooms;

        private readonly List<Room> _rooms = new();
        private readonly Room?[,] _grid;

        private DungeonLayout(int gridSize)
        {
            GridSize = gridSize;
            _grid = new Room?[gridSize, gridSize];
        }

        public Room? At(int col, int row) =>
            col >= 0 && row >= 0 && col < GridSize && row < GridSize ? _grid[col, row] : null;

        public static DungeonLayout Generate(int gridSize, int roomCount, Random rng, double loopChance = 0.1)
        {
            if (gridSize < 1) throw new ArgumentOutOfRangeException(nameof(gridSize));
            if (roomCount < 1 || roomCount > gridSize * gridSize) throw new ArgumentOutOfRangeException(nameof(roomCount));

            var layout = new DungeonLayout(gridSize);
            layout.Add(new Room(gridSize / 2, gridSize / 2));

            // Растим дерево: берём случайную комнату со свободным соседом и открываем его.
            var frontier = new List<Room> { layout._rooms[0] };
            while (layout._rooms.Count < roomCount)
            {
                int idx = rng.Next(frontier.Count);
                var from = frontier[idx];
                var free = DirectionExt.All.Where(d => layout.IsFree(from, d)).ToList();
                if (free.Count == 0)
                {
                    frontier.RemoveAt(idx);
                    continue;
                }

                var dir = free[rng.Next(free.Count)];
                var (dx, dz) = dir.Offset();
                var room = new Room(from.Col + dx, from.Row + dz) { Parent = dir.Opposite() };
                layout.Add(room);
                Connect(from, room, dir);
                frontier.Add(room);
            }

            // Петли: иногда соединяем соседние комнаты, которые ещё не связаны.
            foreach (var room in layout._rooms)
            {
                foreach (var dir in new[] { Direction.North, Direction.East })
                {
                    var (dx, dz) = dir.Offset();
                    var other = layout.At(room.Col + dx, room.Row + dz);
                    if (other != null && !room.Ways.Contains(dir) && rng.NextDouble() < loopChance)
                        Connect(room, other, dir);
                }
            }

            return layout;
        }

        private void Add(Room room)
        {
            _rooms.Add(room);
            _grid[room.Col, room.Row] = room;
        }

        private bool IsFree(Room from, Direction dir)
        {
            var (dx, dz) = dir.Offset();
            int col = from.Col + dx, row = from.Row + dz;
            return col >= 0 && row >= 0 && col < GridSize && row < GridSize && _grid[col, row] == null;
        }

        private static void Connect(Room a, Room b, Direction fromAtoB)
        {
            a.Ways.Add(fromAtoB);
            b.Ways.Add(fromAtoB.Opposite());
        }

        // Схема в консоль: # — комната, - и | — проходы. Север сверху.
        public string Render()
        {
            var lines = new List<string>();
            for (int row = GridSize - 1; row >= 0; row--)
            {
                var cells = new System.Text.StringBuilder();
                var links = new System.Text.StringBuilder();
                for (int col = 0; col < GridSize; col++)
                {
                    var room = _grid[col, row];
                    cells.Append(room == null ? '.' : room.Parent == null ? '@' : '#');
                    cells.Append(room != null && room.Ways.Contains(Direction.East) ? '-' : ' ');
                    links.Append(room != null && room.Ways.Contains(Direction.South) ? '|' : ' ');
                    links.Append(' ');
                }
                lines.Add(cells.ToString().TrimEnd());
                if (row > 0) lines.Add(links.ToString().TrimEnd());
            }
            return string.Join(Environment.NewLine, lines);
        }
    }
}
