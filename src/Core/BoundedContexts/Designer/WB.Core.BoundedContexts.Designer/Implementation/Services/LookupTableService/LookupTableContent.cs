using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    [StoredIn(typeof(StoredLookupTable))]
    public class LookupTableContent
    {
        public LookupTableContent(string[] variableNames, LookupTableRow[] rows)
        {
            VariableNames = variableNames;
            Rows = rows;
        }

        public string[] VariableNames { get; set; } 
        public LookupTableRow[] Rows { get; set; }
    }
}
