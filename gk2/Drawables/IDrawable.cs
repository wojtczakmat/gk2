using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace gk2.Drawables
{
    public interface IDrawable
    {
        void Draw(IDrawer drawer);
        IDrawable HitTest(int x, int y);
    }
}
