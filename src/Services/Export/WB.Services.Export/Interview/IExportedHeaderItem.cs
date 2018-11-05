using System;
using System.Collections.Generic;

namespace WB.Services.Export.Interview
{
    public interface IExportedHeaderItem
    {
        Guid PublicKey { get; set; }
        string VariableName { get; set; }

        List<HeaderColumn> ColumnHeaders { get; set; }
    }
}