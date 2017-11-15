using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace gk2.Drawables
{
    public class Vertex : IDrawable
    {
        public Vertex(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; private set; }
        public int Y { get; private set; }

        public void Draw(IDrawer drawer)
        {
            drawer.SetPixel(X, Y);

            for (int i = 0; i < 9; ++i)
            {
                drawer.SetPixel(X - 4 + i, Y - 4 + i, true);
                drawer.SetPixel(X + 4 - i, Y - 4 + i, true);
            }
        }

        public void Move(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex v)
                return v.X == X && v.Y == Y;
            else
                return false;
        }
    }
}
