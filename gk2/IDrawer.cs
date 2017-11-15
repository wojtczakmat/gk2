using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace gk2
{
    public interface IDrawer
    {
        void SetPixel(int x, int y, bool raw = false);

        BitmapSource ObjectColor { get; set; }
        Color LightColor { get; set; }
    }
}
