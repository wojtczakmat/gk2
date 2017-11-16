using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace gk2
{
    public static class BitmapExtensions
    {
        public static int GetPixelOffset(this BitmapSource bitmap, int bitsPerPixel, int width, int x, int y)
        {
            return ((x + (width * y)) * (bitsPerPixel >> 3));
        }

        public static Color GetBitmapPixel(this BitmapSource b, int bitsPerPixel, byte[] pixels, int x, int y)
        {
            var width = b.PixelWidth;
            x %= width;
            y %= b.PixelHeight;

            var offset = b.GetPixelOffset(bitsPerPixel, width, x, y);
            return Color.FromRgb(pixels[offset + 2],
                pixels[offset + 1], pixels[offset]);
        }
    }
}
