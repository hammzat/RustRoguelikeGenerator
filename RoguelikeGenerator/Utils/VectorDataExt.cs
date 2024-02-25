using System.Globalization;

namespace RoguelikeGenerator.Utils
{
    internal static class VectorDataExtension
    {
        public static string VectorData2String(this WorldSerialization.VectorData vectorData)
        {
            return vectorData.x.ToString(CultureInfo.InvariantCulture) + " " + vectorData.y.ToString(CultureInfo.InvariantCulture) + " " + vectorData.z.ToString(CultureInfo.InvariantCulture);
        }
    }
}
