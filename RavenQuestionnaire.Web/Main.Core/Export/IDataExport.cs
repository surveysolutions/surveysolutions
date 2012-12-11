using System;

namespace Main.Core.Export
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IDataExport
    {
        byte[] ExportData(Guid templateGuid, string type);
    }
}