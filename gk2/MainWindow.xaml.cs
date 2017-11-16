using gk2.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
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

        Timer lightTimer;

        Drawer drawer;

        int width => (int)Width;
        int height => (int)Height;

        List<Polygon> polygons = new List<Polygon>();

        bool useNormalMap;

        // values for light versor
        double r, cx, cy, a, h; // r - radius, cx, cy - origin, a - angle, h - height

        Vertex currentVertex;

        public MainWindow()
        {
            InitializeComponent();

            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            drawer = new Drawer(width, height, bitmap);
            drawer.Init();
            image.Source = bitmap;

            var polygon = new Polygon();
            polygon.AddVertex(100, 100);
            polygon.AddVertex(200, 400);
            polygon.AddVertex(400, 300);
            polygon.AddVertex(150, 90);
            polygon.Close();

            polygons.Add(polygon);

            polygon.IsFilled = true;

            drawer.Drawables.Add(polygon);

            polygon = new Polygon();
            polygon.AddVertex(500, 500);
            polygon.AddVertex(600, 550);
            polygon.AddVertex(550, 650);
            polygon.Close();

            polygons.Add(polygon);

            polygon.IsFilled = true;

            drawer.Drawables.Add(polygon);

            drawer.ObjectColor = LoadBitmap(new Uri("pack://application:,,,/gk2;component/Resources/brick_normalmap.png", UriKind.Absolute));
            drawer.NormalMap = LoadBitmap(new Uri("pack://application:,,,/gk2;component/Resources/normal_map.jpg", UriKind.Absolute));
            drawer.HeightMap = LoadBitmap(new Uri("pack://application:,,,/gk2;component/Resources/brick_heightmap.png", UriKind.Absolute));
            drawer.UseNormalMap = drawer.UseHeightMap = true;

            drawer.Redraw();

            cx = 200;
            cy = 200;
            r = 50;
            a = 0;
            h = 1;

            lightTimer = new Timer();
            lightTimer.Elapsed += LightTimer_Elapsed;
            lightTimer.AutoReset = true;
            lightTimer.Interval = 250;
            lightTimer.Start();

            drawer.UseLightPoint = true;
        }

        private void LightTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            a += 0.1;
            if (h >= 200)
                h = 0;
            if (a >= 2 * Math.PI)
            {
                a = 0;
                h += 10;
            }
            var x = cx + r * Math.Cos(a);
            var y = cy + r * Math.Sin(a);

            Console.WriteLine((x,y));

            image.Dispatcher.Invoke(() =>
            {
                drawer.LightPoint = (x, y, h);

                drawer.Redraw();
            });
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

        private BitmapSource LoadBitmap(Uri path)
        {
            var bmap = new BitmapImage(path);
            if (bmap.Format != PixelFormats.Bgra32)
                return new FormatConvertedBitmap(bmap, PixelFormats.Bgra32, null, 0);
            else
                return bmap;
        }

        private Uri GetBitmapPathFromDialog()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                return new Uri(filename, UriKind.Absolute);
            }
            else
                return null;
        }

        private void LoadObjectTextureButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = GetBitmapPathFromDialog();
            if (uri == null) return;

            drawer.ObjectColor = LoadBitmap(uri);
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
            var uri = GetBitmapPathFromDialog();
            if (uri == null) return;

            drawer.NormalMap = LoadBitmap(uri);
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
            if (drawer == null)
                return;
            drawer.UseHeightMap = true;
            drawer.Redraw();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var uri = GetBitmapPathFromDialog();
            if (uri == null) return;

            drawer.HeightMap = LoadBitmap(uri);
            drawer.Redraw();
            HeightMapTextureRadio.IsEnabled = true;
        }

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(image);
                if (currentVertex == null)
                {
                    var d = drawer.HitTest((int)pos.X, (int)pos.Y);
                    if (d is Vertex v)
                    {
                        currentVertex = v;
                    }
                }
                else
                {
                    currentVertex.Move((int)pos.X, (int)pos.Y);
                    drawer.Redraw();
                }
            }
            else
            {
                currentVertex = null;
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (polygons.Count < 2)
            {
                MessageBox.Show("Za mało wielokątów do obcięcia");
                return;
            }

            try
            {
                var clipping = new Clipping();
                var polys = clipping.Clip(polygons[0], polygons[1]);

                drawer.Drawables.Remove(polygons[0]);
                drawer.Drawables.Remove(polygons[1]);
                polygons.RemoveRange(0, 2);

                foreach (var list in polys)
                {
                    var polygon = new Polygon();
                    foreach (var v in list)
                        polygon.AddVertex((int)v.X, (int)v.Y);
                    polygon.Close();
                    polygon.IsFilled = true;

                    polygons.Add(polygon);
                    drawer.Drawables.Add(polygon);
                }
                drawer.Redraw();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wielokąty nie mają części wspólnej lub są niepoprawne");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var polygon = new Polygon();
            polygon.AddVertex(100, 100);
            polygon.AddVertex(200, 200);
            polygon.AddVertex(300, 250);
            polygon.AddVertex(250, 300);
            polygon.AddVertex(150, 220);
            polygon.Close();
            polygon.IsFilled = true;

            polygons.Add(polygon);
            drawer.Drawables.Add(polygon);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(DistortionTextbox.Text, out double res))
            {
                drawer.DistortionCoeeficient = res;
                drawer.Redraw();
            }
        }
    }
}
