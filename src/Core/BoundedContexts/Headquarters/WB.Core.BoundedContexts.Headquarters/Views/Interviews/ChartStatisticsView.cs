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
    }

    public class ChartStatisticsDataSet
    {
        [Newtonsoft.Json.JsonIgnore]
        internal bool AllZeros { get; set; } = true;
        public InterviewStatus Status { get; set; }
        public List<Point> Data { get; set; } = new List<Point>();
        
        public void Add(object x, long y)
        {
            if (y != 0) AllZeros = false;

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
            public Point(object x, long y)
            {
                X = x;
                Y = y;
            }

            [Newtonsoft.Json.JsonProperty("x")]
            public object X { get; set; }

            [Newtonsoft.Json.JsonProperty("y")]
            public long Y { get; set; }
        }
    }
}
