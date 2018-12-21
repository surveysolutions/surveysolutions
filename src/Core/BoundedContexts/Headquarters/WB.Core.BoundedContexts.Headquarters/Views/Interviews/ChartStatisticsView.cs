using System;
using System.Collections.Generic;
using System.Linq;
using Supercluster.KDBush;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interviews
{
    public class ChartStatisticsView
    {
        public List<ChartStatisticsDataSet> DataSets { get; set; } = new List<ChartStatisticsDataSet>();
        public string From { get; set; }
        public string To { get; set; }
        public string StartDate { get; set; }

        public void FillGaps(DateTime? min, DateTime? max)
        {
            foreach (var chartStatisticsDataSet in DataSets)
            {
                chartStatisticsDataSet.FillGaps(min, max);
            }
        }
    }

    public class ChartStatisticsDataSet
    {
        public InterviewStatus Status { get; set; }
        public List<Point> Data { get; set; } = new List<Point>();

        public void FillGap(object x)
        {
            if (Data.Count > 0)
            {
                if (Data.Last().X.Equals(x))
                {
                    return;
                }

                Add(x, Data.Last().Y);
            }
            else
            {
                Add(x, 0);
            }
        }

        public void Add(object x, object y)
        {
            if (Data.Count > 0 && Data.Last().X.Equals(x))
            {
                Data.Last().Y = y;
            }
            else
            {
                Data.Add(new Point(x, y));
            }
        }

        public class Point
        {
            public Point(object x, object y)
            {
                X = x;
                Y = y;
            }

            [Newtonsoft.Json.JsonProperty("x")]
            public object X { get; set; }

            [Newtonsoft.Json.JsonProperty("y")]
            public object Y { get; set; }
        }
        
        public void FillGaps(DateTime? min, DateTime? max)
        {
            if (min == null) return;

            var current = min;

            foreach (var point in Data)
            {
                    //if(point.)
            }
        }
    }
}
