using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public interface IExportedHeaderItem
    {
        Guid PublicKey { get; set; }
        string[] Titles { get; set; }
        string[] ColumnNames { get; set; }
        string VariableName { get; set; }
    }
}