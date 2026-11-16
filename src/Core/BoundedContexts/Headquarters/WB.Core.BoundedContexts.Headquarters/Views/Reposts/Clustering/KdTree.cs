// Port of https://github.com/marchello2000/kdbush
//
// MIT License
//
// Copyright (c) 2017 Mark Vulfson
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Clustering
{
    public class ClusterPoint
    {
        public ClusterPoint(double x, double y, Cluster userData)
        {
            X = x;
            Y = y;
            UserData = userData;
        }

        public double X { get; }
        public double Y { get; }
        public Cluster UserData { get; }

        public double Longitude => (X - 0.5) * 360;

        public double Latitude
        {
            get
            {
                var y2 = (180 - Y * 360) * Math.PI / 180;
                return 360 * Math.Atan(Math.Exp(y2)) / Math.PI - 90;
            }
        }
    }

    internal class KdTree
    {
        private readonly int nodeSize;
        private readonly List<ClusterPoint> points = new List<ClusterPoint>();
        private readonly Dictionary<int, ClusterPoint> originPoints = new Dictionary<int, ClusterPoint>();

        public KdTree(int nodeSize)
        {
            this.nodeSize = nodeSize;
        }

        public void Index(IEnumerable<ClusterPoint> source)
        {
            var index = 0;
            foreach (var point in source)
            {
                originPoints[index++] = point;
                points.Add(point);
            }

            Sort(0, points.Count - 1, 0);
        }

        public ClusterPoint GetByOriginIndex(int index) => originPoints.GetValueOrDefault(index);

        public List<ClusterPoint> Query(double x1, double y1, double x2, double y2)
        {
            if (x1 > x2) (x1, x2) = (x2, x1);
            if (y1 > y2) (y1, y2) = (y2, y1);

            var hitPoints = new List<ClusterPoint>();
            var stack = new Stack<(int Start, int End, int Axis)>();
            stack.Push((0, points.Count - 1, 0));

            while (stack.Count > 0)
            {
                var (start, end, axis) = stack.Pop();

                if (end - start <= nodeSize)
                {
                    for (var i = start; i <= end; i++)
                    {
                        var p = points[i];
                        if (p.X >= x1 && p.X <= x2 && p.Y >= y1 && p.Y <= y2)
                            hitPoints.Add(p);
                    }
                    continue;
                }

                var m = (start + end) / 2;
                var mid = points[m];

                if (mid.X >= x1 && mid.X <= x2 && mid.Y >= y1 && mid.Y <= y2)
                    hitPoints.Add(mid);

                var nextAxis = 1 - axis;

                if (axis == 0 ? x1 <= mid.X : y1 <= mid.Y)
                    stack.Push((start, m - 1, nextAxis));
                if (axis == 0 ? x2 >= mid.X : y2 >= mid.Y)
                    stack.Push((m + 1, end, nextAxis));
            }

            return hitPoints;
        }

        public List<ClusterPoint> Query(double x, double y, double radius)
        {
            var hitPoints = new List<ClusterPoint>();
            var stack = new Stack<(int Start, int End, int Axis)>();
            stack.Push((0, points.Count - 1, 0));
            var r2 = radius * radius;

            while (stack.Count > 0)
            {
                var (start, end, axis) = stack.Pop();

                if (end - start <= nodeSize)
                {
                    for (var i = start; i <= end; i++)
                    {
                        if (SquareDistance(points[i].X, points[i].Y, x, y) <= r2)
                            hitPoints.Add(points[i]);
                    }
                    continue;
                }

                var m = (start + end) / 2;
                var mid = points[m];

                if (SquareDistance(mid.X, mid.Y, x, y) <= r2)
                    hitPoints.Add(mid);

                var nextAxis = 1 - axis;

                if (axis == 0 ? x - radius <= mid.X : y - radius <= mid.Y)
                    stack.Push((start, m - 1, nextAxis));
                if (axis == 0 ? x + radius >= mid.X : y + radius >= mid.Y)
                    stack.Push((m + 1, end, nextAxis));
            }

            return hitPoints;
        }

        private void Sort(int left, int right, int depth)
        {
            if (right - left <= nodeSize) return;

            var m = (left + right) / 2;

            Select(m, left, right, depth % 2);

            Sort(left, m - 1, depth + 1);
            Sort(m + 1, right, depth + 1);
        }

        // Floyd-Rivest selection
        private void Select(int k, int left, int right, int inc)
        {
            double Coordinate(int index) => inc == 0 ? points[index].X : points[index].Y;

            while (right > left)
            {
                if (right - left > 600)
                {
                    var n = right - left + 1;
                    var m = k - left + 1;
                    var z = Math.Log(n);

                    var s = 0.5 * Math.Exp(2 * z / 3);
                    var sd = 0.5 * Math.Sqrt(z * s * (n - s) / n) * (m - n / 2 < 0 ? -1 : 1);
                    var newLeft = (int)Math.Max(left, k - m * s / n + sd);
                    var newRight = (int)Math.Min(right, k + (n - m) * s / n + sd);

                    Select(k, newLeft, newRight, inc);
                }

                var t = Coordinate(k);
                var i = left;
                var j = right;

                Swap(left, k);
                if (Coordinate(right) > t)
                    Swap(left, right);

                while (i < j)
                {
                    Swap(i, j);
                    i++;
                    j--;

                    while (Coordinate(i) < t) i++;
                    while (Coordinate(j) > t) j--;
                }

                if (Coordinate(left) == t)
                {
                    Swap(left, j);
                }
                else
                {
                    j++;
                    Swap(j, right);
                }

                if (j <= k) left = j + 1;
                if (k <= j) right = j - 1;
            }
        }

        private void Swap(int i1, int i2) => (points[i1], points[i2]) = (points[i2], points[i1]);

        private static double SquareDistance(double x1, double y1, double x2, double y2)
        {
            var dx = x1 - x2;
            var dy = y1 - y2;
            return dx * dx + dy * dy;
        }
    }
}
