using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    [StoredIn(typeof(StoredLookupTable))]
    public class LookupTableContent
    {
        public string[] VariableNames { get; set; } 
        public LookupTableRow[] Rows { get; set; }
    }
}
