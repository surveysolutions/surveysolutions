using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    public class LookupTableRow
    {
        public long RowCode { get; set; }
        public decimal[] Variables { get; set; }
    }
}