using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class ExportServiceDataProvider : IExportServiceDataProvider
    {
        public ExportServiceDataProvider(DiagnosticsExporter diagnosticsExporter,
            IInterviewErrorsExporter interviewErrorsExporter,
            InterviewActionsExporter interviewActionsExporter,
            CommentsExporter commentsExporter)
        {
            serviceLabels.Add(diagnosticsExporter.DiagnosticsFileName, diagnosticsExporter.DiagnosticsFileColumns.ToDictionary(x => x.Title, x => x.Description));
            serviceLabels.Add(interviewErrorsExporter.GetFileName(), interviewErrorsExporter.GetHeader().ToDictionary(x => x.Title, x => x.Description));
            serviceLabels.Add(interviewActionsExporter.InterviewActionsFileName, interviewActionsExporter.ActionFileColumns.ToDictionary(x => x.Title, x => x.Description));
            serviceLabels.Add(commentsExporter.CommentsFileName, commentsExporter.CommentsFileColumns.ToDictionary(x => x.Title, x => x.Description));
        }

        private readonly Dictionary<string, Dictionary<string, string>> serviceLabels = new Dictionary<string, Dictionary<string, string>>();

        public Dictionary<string, Dictionary<string, string>> GetServiceDataLabels()
        {
            return serviceLabels;
        }
    }
}
