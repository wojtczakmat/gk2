using gk2.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gk2
{
    public class Clipping
    {
        public class Point
        {
            public double X { get; set; }
            public double Y { get; set; }

            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }

            public override bool Equals(object obj)
            {
                if (obj is Point p)
                {
                    return this.X == p.X && this.Y == p.Y;
                }
                return false;
            }

            public static implicit operator Point(Vertex v)
            {
                return new Point(v.X, v.Y);
            }
        }
        
        private List<Point> CalculateIntersectionPoints(IReadOnlyList<Edge> p, IReadOnlyList<Edge> q, out List<Point> intersectionPoints)
        {
            intersectionPoints = new List<Point>();
            List<Point> allPoints = new List<Point>();
            foreach (var ep in p)
            {
                List<Point> pointsOnEdge = new List<Point>();
                allPoints.Add(ep.Begin);
                foreach (var eq in q)
                {
                    var ip = IntersectionPoint(ep.Begin, ep.End, eq.Begin, eq.End);
                    if (ip != null)
                        pointsOnEdge.Add(ip);
                }

                if (ep.Begin.X > ep.End.X)
                {
                    pointsOnEdge.Sort((x1, x2) => (x1.X).CompareTo(x2.X));
                    pointsOnEdge.Reverse();
                }
                else if (ep.Begin.X < ep.End.X)
                    pointsOnEdge.Sort((x1, x2) => (x1.X).CompareTo(x2.X));
                else if (ep.Begin.Y > ep.End.Y)
                {
                    pointsOnEdge.Sort((x1, x2) => (x1.Y).CompareTo(x2.Y));
                    pointsOnEdge.Reverse();
                }
                else if (ep.Begin.Y < ep.End.Y)
                    pointsOnEdge.Sort((x1, x2) => (x1.Y).CompareTo(x2.Y));

                intersectionPoints.AddRange(pointsOnEdge);
                allPoints.AddRange(pointsOnEdge);
            }

            return allPoints;
        }


        private List<Point> FindEnteringPoints(List<Point> pointsList, List<Point> ip)
        {
            List<Point> enteringPoints = new List<Point>();
            bool isInside = false;
            foreach (var p in pointsList)
            {
                if (ip.Contains(p))
                {
                    if (!isInside)
                    {
                        enteringPoints.Add(p);
                        isInside = true;
                    }
                    else isInside = false;
                }
            }
            return enteringPoints;
        }

        public List<Point> GetPointsFromVertices(IReadOnlyList<Vertex> vertices)
        {
            return vertices.Select(x => (Point)x).ToList();
        }

        public List<List<Point>> Clip(Polygon polygonP, Polygon polygonQ)
        {
            List<Point> p = GetPointsFromVertices(polygonP.Vertices);
            List<Point> q = GetPointsFromVertices(polygonQ.Vertices);

            List<Point> intersectionPoints;
            p = CalculateIntersectionPoints(polygonP.Edges, polygonQ.Edges, out intersectionPoints);
            q = CalculateIntersectionPoints(polygonQ.Edges, polygonP.Edges, out intersectionPoints);

            double direction = CrossProduct(polygonP.Edges[0].End, polygonP.Edges.Last().Begin, polygonP.Edges[0].Begin);
            if (direction > 0)
                p.Reverse();

            direction = CrossProduct(polygonQ.Edges[0].End, polygonQ.Edges.Last().Begin, polygonQ.Edges[0].Begin);
            if (direction > 0)
                q.Reverse();

            List<Point> enteringPointsP = FindEnteringPoints(p, intersectionPoints);

            List<List<Point>> polygons = new List<List<Point>>();
            List<Point> polygon = new List<Point>();

            Point point = enteringPointsP[0];
            List<Point> currentList = p;
            List<Point> currentIntersectionPoints = new List<Point>();
            polygon.Add(point);
            currentIntersectionPoints.Add(point);
            while (enteringPointsP.Count > 0)
            {
                int k = currentList.IndexOf(point) + 1;
                if (k == currentList.Count)
                    k = 0;

                point = currentList[k];
                if (polygon[0].Equals(point))
                {
                    enteringPointsP.RemoveAll(x => currentIntersectionPoints.Contains(x));
                    currentIntersectionPoints.Clear();
                    polygons.Add(new List<Point>(polygon));
                    polygon = new List<Point>();
                    if (enteringPointsP.Count > 0)
                    {
                        point = enteringPointsP[0];
                        currentIntersectionPoints.Add(point);
                        polygon.Add(point);
                    }
                    else break;
                }
                else
                    polygon.Add(point);

                if (intersectionPoints.Contains(point))
                {
                    currentIntersectionPoints.Add(point);

                    if (currentList == p)
                        currentList = q;
                    else currentList = p;
                }
            }
            return polygons;
        }

        private double CrossProduct(Point p0, Point p1)
        {
            return (p0.X * p1.Y) - (p0.Y * p1.X);
        }

        private double CrossProduct(Point p0, Point p1, Point p2)
        {
            return (p1.X - p0.X) * (p2.Y - p0.Y) - (p2.X - p0.X) * (p1.Y - p0.Y);
        }

        public Point IntersectionPoint(Point p1, Point p2, Point q1, Point q2)
        {
            Point r = Vector(p1, p2);
            Point s = Vector(q1, q2);

            double rs = CrossProduct(r, s);
            if (rs != 0)
            {
                double t = CrossProduct(new Point(q1.X - p1.X, q1.Y - p1.Y), s) / rs;
                double u = CrossProduct(new Point(q1.X - p1.X, q1.Y - p1.Y), r) / rs;

                if (0 <= t && t <= 1 && 0 <= u && u <= 1)
                    return new Point((int)(p1.X + t * r.X), (int)(p1.Y + t * r.Y));
            }

            return null;
        }

        public Point Vector(Point p1, Point p2) => new Point(p2.X - p1.X, p2.Y - p1.Y);
    }
}
