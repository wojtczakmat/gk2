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
    }
}
