using System;
using System.Threading.Tasks;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    internal class DataExportStatusReader : IDataExportStatusReader
    {
        private readonly InterviewDataExportSettings settings;
        private readonly IExportFileNameService ExportFileNameService;

        public DataExportStatusReader(InterviewDataExportSettings settings, 
            IExportFileNameService exportFileNameService)
        {
            this.settings = settings;
            this.ExportFileNameService = exportFileNameService;
        }

        public async Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(string baseUrl,
            string apiKey,
            QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var api = RestService.For<IExportServiceApi>(settings.ExportServiceUrl);
            var archiveFileName = ExportFileNameService.GetQuestionnaireTitleWithVersion(questionnaireIdentity);
            var result = await api.GetDataExportStatusForQuestionnaireAsync(questionnaireIdentity.ToString(),
                archiveFileName, status, fromDate, toDate, apiKey, baseUrl);
            return result;
        }
    }
}
