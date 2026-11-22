using System;
using System.Collections.Generic;
using System.Drawing;

namespace GSTHD
{
    public static class ImageCache
    {
        private static readonly Dictionary<string, Image> Cache =
            new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public static Image Get(string fileName)
        {
            if (Cache.TryGetValue(fileName, out var cached))
                return cached;

            Image copy;
            using (var fromFile = Image.FromFile(@"Resources/" + fileName))
                copy = new Bitmap(fromFile);

            Cache[fileName] = copy;
            return copy;
        }

        public static void Clear()
        {
            Cache.Clear();
        }
    }
}
