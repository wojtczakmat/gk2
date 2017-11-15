using gk2.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gk2
{
    public class ActiveEdgeTable
    {
        IDrawer drawer;
        List<ActiveEdge> list;
        ActiveEdge head, tail;

        public ActiveEdgeTable(IDrawer drawer)
        {
            this.drawer = drawer;
            list = new List<ActiveEdge>();
        }

        public void Add(Edge e)
        {
            var ae = new ActiveEdge(e);
            list.Add(ae);
            return;

            //var ae = new ActiveEdge(e);
            if (head == null)
            {
                tail = head = ae;
                return;
            }

            var p = head;
            while (p.Next != null && p.Next.X < ae.X)
                p = p.Next;
            
            if (p.Next == null)
            {
                tail.Next = ae;
                tail = ae;
            }
            else if(p == head) {
                ae.Next = head;
                head = ae;
            } else
            {
                ae.Next = p.Next.Next;
                p.Next = ae;
            }
        }

        public void Remove(Edge e)
        {
            var toRemove = list.Where(x => x.Edge == e).FirstOrDefault();
            if (toRemove != null)
                list.Remove(toRemove);
            return;

            var p = head;

            if (head.Edge == e)
            {
                head = head.Next;
                return;
            }

            while (p.Next != null)
            {
                if (p.Next.Edge == e)
                {
                    if (p.Next == tail)
                        tail = p;
                    p.Next = p.Next.Next;
                    return;
                }
            }
        }

        public void Update(int y)
        {
            list.Sort((a, b) => a.X.CompareTo(b.X));

            for (int i = 0; i < list.Count - 1; i += 2)
            {
                var x0 = list[i].X;
                var x1 = list[i + 1].X;

                for (var x = x0; x <= x1; ++x)
                    drawer.SetPixel((int)x, y);
            }

            foreach (var e in list)
                e.X += e.OneMth;

            //var p = head;
            //while (p != null && p.Next != null)
            //{
            //    var x0 = p.CrossWithLine(y);
            //    var x1 = p.Next.CrossWithLine(y);

            //    for (int x = x0 + 1; x < x1; ++x)
            //        drawer.SetPixel(x, y);

            //    p = p.Next.Next;
            //}

            //p = head;
            //while (p != null)
            //{
            //    p.X += p.OneMth;
            //    p = p.Next;
            //}
        }

        class ActiveEdge
        {
            public ActiveEdge(Edge e)
            {
                this.YMax = Math.Max(e.Begin.Y, e.End.Y);

                if (e.Begin.Y == e.End.Y)
                    this.OneMth = 0;
                else
                    this.OneMth = ((double)(e.End.X - e.Begin.X)) / (e.End.Y - e.Begin.Y);

                this.X = e.Begin.Y < e.End.Y ? e.Begin.X : e.End.X;
                this.Edge = e;
            }

            public ActiveEdge(int yMax, int xMin, double oneMth)
            {
                this.YMax = YMax;
                this.X = xMin;
                this.OneMth = oneMth;
            }

            public int YMax { get; set; }
            public double X { get; set; }
            public double OneMth { get; set; }

            public Edge Edge { get; set; }

            public ActiveEdge Next { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is ActiveEdge ae)
                {
                    return ae.YMax == this.YMax && ae.OneMth == this.OneMth && ae.X == this.X;
                }
                return false;
            }

            public int CrossWithLine(int y)
            {
                if (Edge.Begin.Y == Edge.End.Y)
                    return Edge.Begin.X;
                if (Edge.Begin.X == Edge.End.X)
                    return Edge.Begin.X;

                var m = 1.0 / OneMth;

                var b = Edge.End.Y - Edge.End.X * m;

                return (int)Math.Round((y - b) / m);
            }
        }
    }
}
