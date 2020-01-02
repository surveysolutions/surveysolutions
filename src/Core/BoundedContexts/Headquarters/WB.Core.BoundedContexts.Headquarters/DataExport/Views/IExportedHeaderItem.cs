using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public interface IExportedHeaderItem
    {
        Guid PublicKey { get; set; }
        string VariableName { get; set; }

        List<HeaderColumn> ColumnHeaders { get; set; }
    }
}
