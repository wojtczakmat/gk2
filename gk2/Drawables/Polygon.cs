using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace gk2.Drawables
{
    public class Polygon : IDrawable
    {
        private List<Edge> edges = new List<Edge>();
        private List<Vertex> vertices = new List<Vertex>();

        public IReadOnlyList<Edge> Edges => edges;
        public IReadOnlyList<Vertex> Vertices => vertices;

        public bool IsClosed { get; private set; }
        public bool IsFilled { get; set; }

        public Polygon()
        { }

        public IDrawable HitTest(int x, int y)
        {
            foreach (var v in vertices)
            {
                var res = v.HitTest(x, y);
                if (res != null)
                    return res;
            }

            foreach (var e in edges)
            {
                var res = e.HitTest(x, y);
                if (res != null)
                    return res;
            }

            return null;
        }

        private void Fill(IDrawer drawer)
        {
            var ind = Enumerable.Range(0, vertices.Count).OrderBy(i => vertices[i].Y).ToArray();
            var yMin = vertices[ind[0]].Y;
            var yMax = vertices[ind[ind.Length - 1]].Y;
            ActiveEdgeTable AET = new ActiveEdgeTable(drawer);

            for (int y = yMin; y <= yMax; ++y)
            {
                for (int k = 0; k < ind.Length; ++k)
                {
                    int i = ind[k];
                    int prev = i > 0 ? i - 1 : vertices.Count - 1;
                    int next = i < vertices.Count - 1 ? i + 1 : 0;

                    if (vertices[i].Y > y - 1)
                        break;

                    if (vertices[i].Y == y - 1)
                    {
                        if (vertices[prev].Y >= vertices[i].Y)
                            AET.Add(edges[prev]);
                        else
                            AET.Remove(edges[prev]);

                        if (vertices[next].Y >= vertices[i].Y)
                            AET.Add(edges[i]);
                        else
                            AET.Remove(edges[i]);
                    }
                }

                AET.Update(y);
            }
        }

        public void Draw(IDrawer drawer)
        {
            if (IsFilled)
                Fill(drawer);

            edges.ForEach(x => x.Draw(drawer));
            vertices.ForEach(x => x.Draw(drawer));
        }

        public void AddVertex(int x, int y)
        {
            AddVertex(new Vertex(x, y));
        }

        public void AddVertex(Vertex v)
        {
            if (vertices.Count > 0)
                edges.Add(new Edge(Vertices.Last(), v));
            vertices.Add(v);
        }

        public void Close()
        {
            edges.Add(new Edge(vertices.Last(), vertices.First()));
            IsClosed = true;
        }

        public void Clip(Polygon clipping)
        {
            var list = new List<ClippingPoint>();

            foreach (var v in vertices)
            {
                list.Add(new ClippingPoint
                {
                    X = v.X,
                    Y = v.Y,
                    IsInside = true,
                    IsVertex = true
                });
            }

            foreach (var v in clipping.Vertices)
            {
                var cp = new ClippingPoint
                {
                    X = v.X,
                    Y = v.Y,
                    IsVertex = true
                };
                cp.IsInside = ClippingPoint.IsInPolygon(this, cp);
                list.Add(cp);
            }
        }

        class ClippingPoint
        {
            public int X { get; set; }
            public int Y { get; set; }
            public bool IsVertex { get; set; }
            public bool IsInside { get; set; }

            public static bool IsInPolygon(Polygon polygon, ClippingPoint testPoint)
            {
                bool result = false;
                int j = polygon.Vertices.Count() - 1;
                for (int i = 0; i < polygon.Vertices.Count(); i++)
                {
                    if (polygon.Vertices[i].Y < testPoint.Y && polygon.Vertices[j].Y >= testPoint.Y || 
                        polygon.Vertices[j].Y < testPoint.Y && polygon.Vertices[i].Y >= testPoint.Y)
                    {
                        if (polygon.Vertices[i].X + (testPoint.Y - polygon.Vertices[i].Y) 
                            / (polygon.Vertices[j].Y - polygon.Vertices[i].Y) 
                            * (polygon.Vertices[j].X - polygon.Vertices[i].X) < testPoint.X)
                        {
                            result = !result;
                        }
                    }
                    j = i;
                }
                return result;
            }
        }
    }
}
