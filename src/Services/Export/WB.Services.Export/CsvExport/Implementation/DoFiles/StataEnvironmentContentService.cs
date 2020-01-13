using System.IO;
using System.Linq;
using System.Threading;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Implementation.DoFiles
{
    public class StataEnvironmentContentService : IEnvironmentContentService
    {
        private readonly IInterviewActionsExporter interviewActionsExporter;
        private readonly ICommentsExporter commentsExporter;
        private readonly IInterviewErrorsExporter interviewErrorsExporter;
        private readonly IDiagnosticsExporter diagnosticsExporter;
        private readonly IAssignmentActionsExporter assignmentActionsExporter;
        private readonly IInterviewsDoFilesExporter interviewsDoFilesExporter;

        public StataEnvironmentContentService(
            IInterviewActionsExporter interviewActionsExporter,
            ICommentsExporter commentsExporter,
            IInterviewErrorsExporter interviewErrorsExporter,
            IDiagnosticsExporter diagnosticsExporter,
            IAssignmentActionsExporter assignmentActionsExporter,
            IInterviewsDoFilesExporter interviewsDoFilesExporter)
        {
            this.interviewActionsExporter = interviewActionsExporter;
            this.commentsExporter = commentsExporter;
            this.interviewErrorsExporter = interviewErrorsExporter;
            this.diagnosticsExporter = diagnosticsExporter;
            this.assignmentActionsExporter = assignmentActionsExporter;
            this.interviewsDoFilesExporter = interviewsDoFilesExporter;
        }

        public void CreateEnvironmentFiles(QuestionnaireExportStructure questionnaireExportStructure, string folderPath, CancellationToken cancellationToken)
        {
            interviewsDoFilesExporter.ExportDoFiles(questionnaireExportStructure, folderPath, cancellationToken);
            interviewActionsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
            commentsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
            interviewErrorsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
            diagnosticsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
            assignmentActionsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
        }
    }
}
