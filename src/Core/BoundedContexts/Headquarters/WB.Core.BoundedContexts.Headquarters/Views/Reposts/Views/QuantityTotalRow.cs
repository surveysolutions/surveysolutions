using System;
using System.Linq;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class QuantityTotalRow
    {
        public QuantityTotalRow(long[] quantityByPeriod, long total)
        {
            this.QuantityByPeriod = quantityByPeriod;
            this.Average = quantityByPeriod.Length > 0 ? Math.Round(quantityByPeriod.Average(), 2) : 0;
            this.Total = total;
        }

        public long[] QuantityByPeriod { get; set; }
        public double Average { get; set; }
        public long Total { get; set; }
    }
}