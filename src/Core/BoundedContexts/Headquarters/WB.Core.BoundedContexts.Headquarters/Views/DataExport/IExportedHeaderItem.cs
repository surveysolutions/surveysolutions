using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public interface IExportedHeaderItem
    {
        Guid PublicKey { get; set; }
        string VariableName { get; set; }

        List<HeaderColumn> ColumnHeaders { get; set; }
    }
}
