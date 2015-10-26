using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal interface IQuestionnaireDataExportServiceFactory
    {
        IQuestionnaireDataExportService CreateQuestionnaireDataExportService(DataExportType dataExportType);
    }
}