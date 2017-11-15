using gk2.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace gk2
{
    public class Drawer : IDrawer
    {
        WriteableBitmap bitmap;
        byte[] pixels;
        Color defaultColor;
        int width, height;

        private byte[] objectPixels;
        private BitmapSource objectColor;
        public BitmapSource ObjectColor
        {
            get => objectColor;
            set
            {
                objectColor = value;
                objectPixels = 
                    new byte[objectColor.PixelWidth * objectColor.PixelHeight * objectColor.Format.BitsPerPixel / 8];
                int stride = objectColor.PixelWidth * (objectColor.Format.BitsPerPixel / 8);

                objectColor.CopyPixels(objectPixels, stride, 0);
            }
        }

        private byte[] normalMapPixels;
        private BitmapSource normalMap;
        public BitmapSource NormalMap
        {
            get => normalMap;
            set
            {
                normalMap = value;
                normalMapPixels =
                    new byte[normalMap.PixelWidth * normalMap.PixelHeight * normalMap.Format.BitsPerPixel / 8];
                int stride = normalMap.PixelWidth * (normalMap.Format.BitsPerPixel / 8);

                normalMap.CopyPixels(normalMapPixels, stride, 0);
            }
        }

        private byte[] heightMapPixels;
        private BitmapSource heightMap;
        public BitmapSource HeightMap
        {
            get => heightMap;
            set
            {
                heightMap = value;
                heightMapPixels =
                    new byte[heightMap.PixelWidth * heightMap.PixelHeight * heightMap.Format.BitsPerPixel / 8];
                int stride = heightMap.PixelWidth * (heightMap.Format.BitsPerPixel / 8);

                HeightMap.CopyPixels(heightMapPixels, stride, 0);
            }
        }

        public bool UseNormalMap { get; set; }
        public bool UseHeightMap { get; set; }

        public Color LightColor { get; set; } = Colors.White;
        public (double x, double y, double z) LightVersor { get; set; } = (0, 0, 1);
        public (double x, double y, double z) NormalVector { get; set; } = (0, 0, 1);
        public (int x, int y, int z) DistortionVector { get; set; } = (0, 0, 0);

        public List<IDrawable> Drawables { get; set; } = new List<IDrawable>();

        public Drawer(int width, int height, WriteableBitmap bitmap)
        {
            this.bitmap = bitmap;
            this.pixels = new byte[width * height * (bitmap.Format.BitsPerPixel / 8)];

            this.width = width;
            this.height = height;

            this.defaultColor = Colors.Black;

            var wb = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            unsafe
            {
                int pBackBuffer = (int)wb.BackBuffer;
                int color_data = 255 << 16; // R
                color_data |= 0 << 8;   // G
                color_data |= 0 << 0;   // B
                *((int*)pBackBuffer) = color_data;
            }

            //wb.WritePixels(new Int32Rect(0, 0, 1, 1), new[] { (byte)0, (byte)0, (byte)255, 0xFF }, wb.Format.BitsPerPixel / 8, 0);
            ObjectColor = wb;
        }

        public void SetPixel(int x, int y, bool raw = false)
        {
            Color color = new Color();
            if (raw)
            {
                color = Color.FromRgb(0, 0, 0);
            }
            else
            {
                var bitmapPixel = ObjectColor.GetBitmapPixel(objectPixels, x, y);
                var NPrim = GetNPrim(x,y);
                var r = bitmapPixel.R * LightColor.R * CosNPrimL(NPrim) / 255.0;
                var g = bitmapPixel.G * LightColor.G * CosNPrimL(NPrim) / 255.0;
                var b = bitmapPixel.B * LightColor.B * CosNPrimL(NPrim) / 255.0;

                color = Color.FromRgb((byte)r, (byte)g, (byte)b);

            }

            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            
            int pixelOffset = bitmap.GetPixelOffset(x, y);
        
            pixels[pixelOffset] = color.B;
            pixels[pixelOffset + 1] = color.G;
            pixels[pixelOffset + 2] = color.R;
            pixels[pixelOffset + 3] = color.A;
        }

        private (double, double, double) GetNPrim(int xPix, int yPix)
        {
            double x, y, z;
            (x, y, z) = NormalVector;
            if (UseNormalMap)
            {
                var c = normalMap.GetBitmapPixel(normalMapPixels, xPix, yPix);
                x = (2 * (c.R / 255.0)) - 1;
                y = (2 * (c.G / 255.0)) - 1;
                z = c.B / 255.0;
            }
            
            var D = new double[]{0, 0, 0 };
            if (UseHeightMap)
            {
                var rightPixel = heightMap.GetBitmapPixel(heightMapPixels, xPix+1, yPix);
                var middlePixel = heightMap.GetBitmapPixel(heightMapPixels, xPix, yPix);
                var upPixel = heightMap.GetBitmapPixel(heightMapPixels, xPix, yPix+1);

                D = new double[]{
                    rightPixel.R - middlePixel.R,
                    upPixel.G - middlePixel.G,
                    -x * (rightPixel.B - middlePixel.B) + -y * (upPixel.B - middlePixel.B)
                };
            }

            (x,y,z) = (x + D[0], y + D[1], z+D[2]);
            return Normalize(x, y, z);
        }

        private (double, double, double) Normalize(double x, double y, double z)
        {
            var length = Math.Sqrt(x * x + y * y + z * z);
            return (x / length, y / length, z / length);
        }

        private double CosNPrimL((double x, double y, double z) NPrim)
        {
            var L = LightVersor;

            return NPrim.x * L.x + NPrim.y * L.y + NPrim.z * L.z;
        }

        public void Init()
        {
            for (int j = 0; j < height; ++j)
            {
                for (int i = 0; i < width; ++i)
                {
                    int blue = 255;
                    int green = 50;
                    int red = 50;
                    int alpha = 255;

                    int pixelOffset = bitmap.GetPixelOffset(i, j);

                    pixels[pixelOffset] = (byte)blue;
                    pixels[pixelOffset + 1] = (byte)green;
                    pixels[pixelOffset + 2] = (byte)red;
                    pixels[pixelOffset + 3] = (byte)alpha;
                }
            }

            Redraw();
        }


        public void Redraw()
        {
            WipePixels();

            foreach (var drawable in Drawables)
            {
                drawable.Draw(this);
            }

            int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);

            Int32Rect rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);
        }

        private void WipePixels()
        {
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i] = 0x0;
        }
    }
}
