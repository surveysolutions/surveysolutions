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
            ICommentsExporter commentsExporter,
            IAssignmentActionsExporter assignmentActionsExporter)
        {
            serviceLabels.Add(diagnosticsExporter.DiagnosticsFileName, 
                diagnosticsExporter.DiagnosticsFileColumns.ToDictionary(x => x.Title, x => new HeaderItemDescription(x.Description, x.ValueType, x.VariableValueLabels)));
            serviceLabels.Add(interviewErrorsExporter.GetFileName(), 
                interviewErrorsExporter.GetHeader().ToDictionary(x => x.Title, x => new HeaderItemDescription(x.Description, x.ValueType, x.VariableValueLabels)));
            serviceLabels.Add(interviewActionsExporter.InterviewActionsFileName, 
                interviewActionsExporter.ActionFileColumns.ToDictionary(x => x.Title, x => new HeaderItemDescription(x.Description, x.ValueType, x.VariableValueLabels)));
            serviceLabels.Add(commentsExporter.CommentsFileName, 
                commentsExporter.CommentsFileColumns.ToDictionary(x => x.Title, x => new HeaderItemDescription(x.Description, x.ValueType, x.VariableValueLabels)));
            serviceLabels.Add(assignmentActionsExporter.AssignmentActionsFileName,
                assignmentActionsExporter.ActionFileColumns.ToDictionary(x => x.Title, x => new HeaderItemDescription(x.Description, x.ValueType, x.VariableValueLabels)));
        }

        private readonly Dictionary<string, Dictionary<string, HeaderItemDescription>> serviceLabels = 
            new Dictionary<string, Dictionary<string, HeaderItemDescription>>();

        public Dictionary<string, Dictionary<string, HeaderItemDescription>> GetServiceDataLabels()
        {
            return serviceLabels;
        }
    }
}
