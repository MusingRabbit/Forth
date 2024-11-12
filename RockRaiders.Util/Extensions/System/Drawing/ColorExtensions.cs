using System.Drawing;

namespace RockRaiders.Util.Extensions.System.Drawing
{
    public static class ColorExtensions
    {
        public static string ToHex(this Color color)
        {
            return $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}";
        }
    }
}
