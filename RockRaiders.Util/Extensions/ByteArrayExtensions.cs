namespace RockRaiders.Util.Extensions
{
    public static class ByteArrayExtensions
    {
        public static byte[] Reverse(this byte[] src)
        {
            var result = new byte[src.Length];

            for (var i = 0; i < src.Length; i++)
            {
                var endIdx = src.Length - (i + 1);
                result[i] = src[endIdx];
            }

            return src;
        }

        public static byte[] Resize(this byte[] src, int size)
        {
            var result = new byte[size];

            for (var i = 0; i < src.Length; i++)
            {
                result[i] = src[i];
            }

            return result;
        }
    }
}
