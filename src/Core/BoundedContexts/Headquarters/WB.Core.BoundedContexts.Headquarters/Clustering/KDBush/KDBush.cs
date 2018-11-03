using System;
using System.Collections.Generic;
using System.Threading;

namespace KDBush
{
    /// <summary>
    /// A very fast static spatial index for 2D points based on a flat KD-tree. 
    /// Unlike R-tree or R-bush, KD-tree supports points only and is static 
    /// (can't add points once the index is constructed).
    /// However, the queries are significantly (5-8x) faster
    /// https://github.com/marchello2000/kdbush Copyright (c) 2017 Mark Vulfson
    /// </summary>
    public class KDBush<T>
    {
        internal readonly int nodeSize;
        internal List<Point<T>> points;

        /// <summary>
        /// Initialize a KDBush
        /// </summary>
        /// <param name="nodeSize">Size of the KD-tree node, 64 by default. Higher means faster indexing but slower search, and vise versa.</param>
        public KDBush(int nodeSize = 128)
        {
            this.nodeSize = nodeSize;
            this.points = new List<Point<T>>();
        }

        /// <summary>
        /// Create an index from the given nodes.
        /// Note, if the index was previously constructed with other points,
        /// those points will be erased
        /// </summary>
        /// <param name="points">List of points to index</param>
        public void Index(IEnumerable<Point<T>> points)
        {
            this.points.Clear();
            this.points.AddRange(points);

            Sort(0, this.points.Count - 1, 0);
        }

        /// <summary>
        /// Query the index for nodes with a rectangle.
        /// Points that lie exactly on the edge of the rectangle are
        /// considered to be within the rectangle and will be returned
        /// </summary>
        /// <param name="x1">min x</param>
        /// <param name="y1">min y</param>
        /// <param name="x2">max x</param>
        /// <param name="y2">max y</param>
        public List<Point<T>> Query(double x1, double y1, double x2, double y2)
        {
            if (x1 > x2) (x1, x2) = (x2, x1);
            if (y1 > y2) (y1, y2) = (y2, y1);

            List<Point<T>> hitPoints = new List<Point<T>>();

            Stack<SearchState> stack = new Stack<SearchState>();
            stack.Push(new SearchState(0, points.Count - 1, SearchAxis.XAxis));
            double x;
            double y;

            while (stack.Count > 0)
            {
                SearchState state = stack.Pop();

                if (state.End - state.Start <= nodeSize)
                {
                    for (var i = state.Start; i <= state.End; i++)
                    {
                        x = points[i].X;
                        y = points[i].Y;

                        if (x >= x1 && x <= x2 && y >= y1 && y <= y2)
                        {
                            hitPoints.Add(points[i]);
                        }
                    }

                    continue;
                }

                int m = (state.Start + state.End) / 2;

                x = points[m].X;
                y = points[m].Y;

                if (x >= x1 && x <= x2 && y >= y1 && y <= y2)
                {
                    hitPoints.Add(points[m]);
                }

                SearchAxis nextAxis = (state.Axis == SearchAxis.XAxis) ? SearchAxis.YAxis : SearchAxis.XAxis;

                if ((state.Axis == SearchAxis.XAxis) ? x1 <= x : y1 <= y)
                {
                    stack.Push(new SearchState(state.Start, m - 1, nextAxis));
                }
                if ((state.Axis == SearchAxis.XAxis) ? x2 >= x : y2 >= y)
                {
                    stack.Push(new SearchState(m + 1, state.End, nextAxis));
                }
            }

            return hitPoints;
        }

        /// <summary>
        /// Query the index for nodes with a circle.
        /// Points that lie exactly on the edge of the circle are
        /// considered to be within the circle and will be returned
        /// </summary>
        /// <param name="x">center X of the circle</param>
        /// <param name="y">center Y of the circle</param>
        /// <param name="radius">radius of the circle</param>
        public List<Point<T>> Query(double x, double y, double radius)
        {
            List<Point<T>> hitPoints = new List<Point<T>>();
            Stack<SearchState> stack = new Stack<SearchState>();
            stack.Push(new SearchState(0, points.Count - 1, SearchAxis.XAxis));
            double r2 = radius * radius;

            while (stack.Count > 0)
            {
                SearchState state = stack.Pop();

                if ((state.End - state.Start) <= nodeSize)
                {
                    for (var i = state.Start; i <= state.End; i++)
                    {
                        if (SquareDistance(points[i].X, points[i].Y, x, y) <= r2)
                        {
                            hitPoints.Add(points[i]);
                        }
                    }

                    continue;
                }

                int m = (state.Start + state.End) / 2;

                double px = points[m].X;
                double py = points[m].Y;

                if (SquareDistance(px, py, x, y) <= r2)
                {
                    hitPoints.Add(points[m]);
                }

                SearchAxis nextAxis = (state.Axis == SearchAxis.XAxis) ? SearchAxis.YAxis : SearchAxis.XAxis;

                if ((state.Axis == SearchAxis.XAxis) ? x - radius <= px : y - radius <= py)
                {
                    stack.Push(new SearchState(state.Start, m - 1, nextAxis));
                }
                if ((state.Axis == SearchAxis.XAxis) ? x + radius >= px : y + radius >= py)
                {
                    stack.Push(new SearchState(m + 1, state.End, nextAxis));
                }
            }

            return hitPoints;
        }

        private void Sort(int left, int right, int depth)
        {
            if ((right - left) <= nodeSize)
            {
                return;
            }

            int m = (left + right) / 2;

            Select(m, left, right, depth % 2);

            Sort(left, m - 1, depth + 1);
            Sort(m + 1, right, depth + 1);
        }

        private void Select(int k, int left, int right, int inc)
        {
            while (right > left)
            {
                if ((right - left) > 600)
                {
                    int n = right - left + 1;
                    int m = k - left + 1;
                    double z = Math.Log(n);

                    double s = 0.5 * Math.Exp(2 * z / 3);
                    double sd = 0.5 * Math.Sqrt(z * s * (n - s) / n) * (m - n / 2 < 0 ? -1 : 1);
                    int newLeft = (int)Math.Max(left, (k - m * s / n + sd));
                    int newRight = (int)Math.Min(right, (k + (n - m) * s / n + sd));

                    Select(k, newLeft, newRight, inc);
                }

                double GetCoordinate(int index)
                {
                    return inc == 0 ? points[index].X : points[index].Y;
                }

                var t = GetCoordinate(k);
                var i = left;
                var j = right;

                Swap(left, k);
                if (GetCoordinate(right) > t)
                {
                    Swap(left, right);
                }

                while (i < j)
                {
                    Swap(i, j);
                    i++;
                    j--;

                    while (GetCoordinate(i) < t)
                    {
                        i++;
                    }

                    while (GetCoordinate(j) > t)
                    {
                        j--;
                    }
                }

                if (GetCoordinate(left) == t)
                {
                    Swap(left, j);
                }
                else
                {
                    j++;
                    Swap(j, right);
                }

                if (j <= k)
                {
                    left = j + 1;
                }
                if (k <= j)
                {
                    right = j - 1;
                }
            }
        }

        private void Swap(int i1, int i2)
        {
            Point<T> temp = points[i1];
            points[i1] = points[i2];
            points[i2] = temp;
        }

        private double SquareDistance(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;

            return (dx * dx) + (dy * dy);
        }

        enum SearchAxis
        {
            XAxis = 0,
            YAxis = 1
        }

        /// <summary>
        /// Class to keep track of search state
        /// </summary>
        class SearchState
        {
            public SearchAxis Axis { get; private set; }
            public int Start { get; private set; }
            public int End { get; private set; }

            public SearchState(int start, int end, SearchAxis axis)
            {
                Axis = axis;
                Start = start;
                End = end;
            }
        }
    }

    public class Point<T>
    {
        public double X { get; }
        public double Y { get; }

        public double Longitude => xLng(X);
        public double Latitude => yLat(Y);

        public T UserData { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point(double x, double y, T userData)
        {
            X = x;
            Y = y;
            UserData = userData;
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}. {UserData.ToString()}";
        }


        // spherical mercator to longitude/latitude
        double xLng(double x)
        {
            return (x - 0.5) * 360;
        }

        double yLat(double y)
        {
            var y2 = (180 - y * 360) * Math.PI / 180;
            return 360 * Math.Atan(Math.Exp(y2)) / Math.PI - 90;
        }
    }
}
