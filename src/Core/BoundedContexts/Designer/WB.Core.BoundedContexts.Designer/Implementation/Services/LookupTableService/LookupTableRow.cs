using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    public class LookupTableRow
    {
        public long RowCode { get; set; }
        public Dictionary<string, decimal> Variables { get; set; }
    }
}