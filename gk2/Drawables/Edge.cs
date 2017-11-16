using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace gk2.Drawables
{
    public class Edge : IDrawable
    {
        public Edge(Vertex begin, Vertex end)
        {
            Begin = begin;
            End = end;
        }

        public Vertex Begin { get; }
        public Vertex End { get; }

        public bool IsHorizontal => Begin.Y == End.Y;
        public bool IsVertical => Begin.X == End.X;

        public Edge Inverse() => new Edge(End, Begin);

        public IDrawable HitTest(int x, int y)
        {
            if (Begin.Y == End.Y)
                return y == Begin.Y ? this : null;
            if (Begin.X == End.X)
                return x == Begin.X ? this : null;

            int m = (End.Y - Begin.Y) / (End.X - Begin.X);
            int b = End.Y - m * End.X;

            if (x * m + b == y)
                return this;
            else
                return null;
        }

        public void Draw(IDrawer drawer)
        {
            int x = Begin.X;
            int y = Begin.Y;
            int x2 = End.X;
            int y2 = End.Y;

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                drawer.SetPixel(x, y, true);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Edge e)
            {
                return e.Begin == this.Begin && e.End == this.End;
            }
            return false;
        }
    }
}
