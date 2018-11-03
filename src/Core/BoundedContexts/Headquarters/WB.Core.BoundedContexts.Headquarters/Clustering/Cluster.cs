using System.Collections.Generic;
using System.Text;

namespace WB.Core.BoundedContexts.Headquarters.Clustering
{
    public class Cluster
    {
        public int Zoom { get; set; }
        public long Index { get; set; }
        public int ParentId { get; set; }
        public int? NumPoints { get; set; }
        public Dictionary<string, object> Props { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if(Props != null)
            foreach (var kv in Props)
            {
                sb.Append($"{kv.Key}: {kv.Value};");
            }
            return $"#{Index} {NumPoints ?? 0} points {sb}";
        }
    }
}
