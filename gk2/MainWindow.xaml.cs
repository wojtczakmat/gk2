using gk2.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Polygon = gk2.Drawables.Polygon;

namespace gk2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        State State { get; set; } = State.Idle;

        readonly WriteableBitmap bitmap;

        Drawer drawer;

        int width => (int)Width;
        int height => (int)Height;

        List<Polygon> polygons = new List<Polygon>();

        bool useNormalMap;

        public MainWindow()
        {
            InitializeComponent();

            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            drawer = new Drawer(width, height, bitmap);
            drawer.Init();
            image.Source = bitmap;

            var polygon = new Polygon();
            polygon.AddVertex(401, 200);
            polygon.AddVertex(600, 400);
            polygon.AddVertex(400, 601);
            polygon.AddVertex(200, 400);
            polygon.Close();

            polygons.Add(polygon);

            polygon.IsFilled = true;

            drawer.Drawables.Add(polygon);

            polygon = new Polygon();
            polygon.AddVertex(100, 100);
            polygon.AddVertex(100, 200);
            polygon.AddVertex(150, 150);
            polygon.AddVertex(210, 210);
            polygon.AddVertex(200, 100);
            polygon.Close();

            polygons.Add(polygon);

            polygon.IsFilled = true;

            drawer.Drawables.Add(polygon);

            drawer.Redraw();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (byte.TryParse(ObjectColorR.Text, out byte r)
                && byte.TryParse(ObjectColorG.Text, out byte g)
                && byte.TryParse(ObjectColorB.Text, out byte b))
            {
                var wb = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);

                unsafe
                {
                    int pBackBuffer = (int)wb.BackBuffer;
                    int color_data = r << 16; // R
                    color_data |= g << 8;   // G
                    color_data |= b << 0;   // B
                    *((int*)pBackBuffer) = color_data;
                }

                //wb.WritePixels(new Int32Rect(0, 0, 1, 1), new[] { (byte)b, (byte)g, (byte)r, 0xFF }, wb.Format.BitsPerPixel / 8, 0);

                drawer.ObjectColor = wb;
                drawer.Redraw();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (byte.TryParse(LightColorR.Text, out byte r)
                && byte.TryParse(LightColorG.Text, out byte g)
                && byte.TryParse(LightColorB.Text, out byte b))
            {
                drawer.LightColor = Color.FromRgb(r, g, b);
                drawer.Redraw();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private BitmapSource LoadBitmap(Uri path)
        {
            var bmap = new BitmapImage(path);
            if (bmap.Format != PixelFormats.Bgra32)
                return new FormatConvertedBitmap(bmap, PixelFormats.Bgra32, null, 0);
            else
                return bmap;
        }

        private void LoadObjectTextureButton_Click(object sender, RoutedEventArgs e)
        {
            //tutaj popup
            drawer.ObjectColor = LoadBitmap(new Uri("pack://application:,,,/gk2;component/Resources/normal_map.jpg", UriKind.Absolute));
            drawer.Redraw();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (drawer == null)
                return;
            drawer.UseNormalMap = false;
            drawer.Redraw();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            if (drawer == null)
                return;
            drawer.UseNormalMap = true;
            drawer.Redraw();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            drawer.NormalMap = LoadBitmap(new Uri("pack://application:,,,/gk2;component/Resources/brick_normalmap.png", UriKind.Absolute));
            drawer.Redraw();
            NormalMapTextureRadio.IsEnabled = true;
        }
        
        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            if (drawer == null)
                return;
            drawer.UseHeightMap = false;
            drawer.Redraw();
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            drawer.UseHeightMap = true;
            drawer.Redraw();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            drawer.HeightMap = LoadBitmap(new Uri("pack://application:,,,/gk2;component/Resources/brick_heightmap.png", UriKind.Absolute));
            drawer.Redraw();
        }
    }
}
