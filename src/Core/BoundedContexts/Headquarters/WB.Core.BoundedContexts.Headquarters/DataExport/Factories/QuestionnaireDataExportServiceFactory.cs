using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class QuestionnaireDataExportServiceFactory: IQuestionnaireDataExportServiceFactory
    {
        public IQuestionnaireDataExportService CreateQuestionnaireDataExportService(DataExportType dataExportType)
        {
            if(dataExportType==DataExportType.Paradata)
                return new QuestionnaireParaDataExportService();

            throw new NotSupportedException();
        }
    }
}