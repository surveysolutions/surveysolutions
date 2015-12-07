using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class LookupTable
    {
        public string FileName { get; set; }
        public HashSet<string> VariableNames { get; set; }
        public List<LookupTable> Rows { get; set; }
    }

    public class LookupTableRecord
    {
        public decimal RowCode { get; set; }
        public Dictionary<string,decimal> Variables { get; set; }
    }
}