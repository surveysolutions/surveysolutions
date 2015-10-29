using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal interface IDataExportService
    {
        void ExportData(Guid questionnaireId, long questionnaireVersion, string dataExportProcessId);
        void ExportParaData(string dataExportProcessId);
    }
}