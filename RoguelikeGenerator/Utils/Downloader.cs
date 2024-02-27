using System.Net;

namespace RoguelikeGenerator.Utils
{
    public class Downloader
    {
        public static void LoadDefaultMaps()
        {
            using (WebClient client = new WebClient())
            {
                Console.WriteLine($"Скачавание _base.map (не удаляйте её никогда)");
                client.DownloadFile("https://github.com/hammzat/RustRoguelikeGenerator/raw/main/maps/_basemap.map",     "maps/_base.map"); // обязательная для генерации
                Console.WriteLine($"Скачавание map_cleared.map (Пустая карта, редактируйте её для создания новых комнат.)");
                client.DownloadFile("https://github.com/hammzat/RustRoguelikeGenerator/raw/main/maps/map_cleared.map",  "maps/map_cleared.map");
                Console.WriteLine($"Скачавание map_room.map (Пример)");
                client.DownloadFile("https://github.com/hammzat/RustRoguelikeGenerator/raw/main/maps/map_room.map",     "maps/map_room.map");
            }
        }
    }
}
