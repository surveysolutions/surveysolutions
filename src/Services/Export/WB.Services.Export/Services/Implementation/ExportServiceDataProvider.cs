using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.CsvExport.Exporters;

namespace WB.Services.Export.Services.Implementation
{
    internal class ExportServiceDataProvider : IExportServiceDataProvider
    {
        public ExportServiceDataProvider(IDiagnosticsExporter diagnosticsExporter,
            IInterviewErrorsExporter interviewErrorsExporter,
            IInterviewActionsExporter interviewActionsExporter,
            ICommentsExporter commentsExporter)
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
