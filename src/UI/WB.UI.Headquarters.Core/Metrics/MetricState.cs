using System;
using System.Collections.Generic;

namespace WB.UI.Headquarters.Metrics
{
    public readonly struct MetricState
    {
        public MetricState(string name, string value, double data)
        {
            Name = name;
            Value = value;
            Data = data;
        }

        public string Name { get; }
        public string Value { get; }
        public double Data { get; }
    }

    public class ServerStatusResponse
    {
        public List<MetricState> Metrics { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
