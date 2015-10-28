using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal interface IDataExportService
    {
        string ExportData(Guid questionnaireId, long questionnaireVersion, string dataExportProcessId);
        string ExportParaData(string dataExportProcessId);
    }
}