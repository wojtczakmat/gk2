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
        public static int GetPixelOffset(this BitmapSource bitmap, int x, int y)
        {
            return ((x + (bitmap.PixelWidth * y)) * (bitmap.Format.BitsPerPixel / 8));
        }

        public static Color GetBitmapPixel(this BitmapSource b, byte[] pixels, int x, int y)
        {
            x %= b.PixelWidth;
            y %= b.PixelHeight;

            var offset = b.GetPixelOffset(x, y);
            return Color.FromArgb(pixels[offset + 3], pixels[offset + 2],
                pixels[offset + 1], pixels[offset]);
        }
    }
}
