namespace RoguelikeGenerator.Utils
{
    // Стороны комнаты в плоскости XZ. North = +Z, East = +X.
    // Порядок важен: поворот на 90° по часовой (вокруг Y) = следующий элемент.
    public enum Direction
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    public static class DirectionExt
    {
        public static readonly Direction[] All = { Direction.North, Direction.East, Direction.South, Direction.West };

        public static (int dx, int dz) Offset(this Direction dir) => dir switch
        {
            Direction.North => (0, 1),
            Direction.East => (1, 0),
            Direction.South => (0, -1),
            Direction.West => (-1, 0),
            _ => (0, 0)
        };

        public static Direction Opposite(this Direction dir) => (Direction)(((int)dir + 2) % 4);

        // Поворот стороны на quarterTurns * 90° по часовой (так же, как Unity поворачивает по +Y).
        public static Direction Rotate(this Direction dir, int quarterTurns) => (Direction)((((int)dir + quarterTurns) % 4 + 4) % 4);

        // Сколько четвертей нужно повернуть from, чтобы получить to.
        public static int TurnsTo(this Direction from, Direction to) => (((int)to - (int)from) % 4 + 4) % 4;
    }
}
