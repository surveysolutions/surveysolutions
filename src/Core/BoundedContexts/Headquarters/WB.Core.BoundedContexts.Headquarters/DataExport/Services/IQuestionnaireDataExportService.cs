using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal interface IQuestionnaireDataExportService
    {
        void Export(Guid questionnaireId, long questionnaireVersion, string dataExportProcessId);
    }
}