using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    public class LookupTableContent
    {
        public string[] VariableNames { get; set; } 
        public LookupTableRow[] Rows { get; set; }
    }
}