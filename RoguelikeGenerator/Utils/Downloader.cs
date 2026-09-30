namespace RoguelikeGenerator.Utils
{
    public static class Downloader
    {
        private const string BaseUrl = "https://github.com/hammzat/RustRoguelikeGenerator/raw/main/maps/";

        private static readonly (string file, string description)[] DefaultMaps =
        {
            ("_base.map", "Базовая карта, на неё кладётся данж (не удаляйте её)"),
            ("map_cleared.map", "Пустая карта, редактируйте её для создания новых комнат"),
            ("map_room.map", "Пример комнаты"),
            ("map_room_ext.map", "Пример комнаты, расширенный"),
        };

        public static void LoadDefaultMaps(string mapsDir)
        {
            using var client = new HttpClient();
            foreach (var (file, description) in DefaultMaps)
            {
                var target = Path.Combine(mapsDir, file);
                if (File.Exists(target)) continue;

                Console.WriteLine($"Скачивание {file} ({description})");
                var bytes = client.GetByteArrayAsync(BaseUrl + file).GetAwaiter().GetResult();
                File.WriteAllBytes(target, bytes);
            }
        }
    }
}
