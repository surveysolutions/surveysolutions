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
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;
        private readonly IInterviewActionsExporter interviewActionsExporter;
        private readonly ICommentsExporter commentsExporter;
        private readonly IInterviewErrorsExporter interviewErrorsExporter;
        private readonly IDiagnosticsExporter diagnosticsExporter;
        private readonly IAssignmentActionsExporter assignmentActionsExporter;

        public StataEnvironmentContentService(
            IFileSystemAccessor fileSystemAccessor,
            IQuestionnaireLabelFactory questionnaireLabelFactory,
            IInterviewActionsExporter interviewActionsExporter,
            ICommentsExporter commentsExporter,
            IInterviewErrorsExporter interviewErrorsExporter,
            IDiagnosticsExporter diagnosticsExporter,
            IAssignmentActionsExporter assignmentActionsExporter)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
            this.interviewActionsExporter = interviewActionsExporter;
            this.commentsExporter = commentsExporter;
            this.interviewErrorsExporter = interviewErrorsExporter;
            this.diagnosticsExporter = diagnosticsExporter;
            this.assignmentActionsExporter = assignmentActionsExporter;
        }

        public void CreateEnvironmentFiles(QuestionnaireExportStructure questionnaireExportStructure, string folderPath, CancellationToken cancellationToken)
        {
            var questionnaireLabels =
                this.questionnaireLabelFactory.CreateLabelsForQuestionnaire(questionnaireExportStructure);

            foreach (var questionnaireLevelLabels in questionnaireLabels)
            {
                cancellationToken.ThrowIfCancellationRequested();

                this.CreateContentOfAdditionalFile(questionnaireLevelLabels,
                    ExportFileSettings.GetContentFileName(questionnaireLevelLabels.LevelName), folderPath);
            }

            interviewActionsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
            commentsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
            interviewErrorsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
            diagnosticsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
            assignmentActionsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
        }

        private void CreateContentOfAdditionalFile(QuestionnaireLevelLabels questionnaireLevelLabels, string dataFileName, string basePath)
        {
            var doContent = new DoFile();

            doContent.BuildInsheet(dataFileName);

            this.BuildLabelsForLevel(doContent, questionnaireLevelLabels);

            var contentFilePath = Path.Combine(basePath,
                this.GetEnvironmentContentFileName(questionnaireLevelLabels.LevelName));

            this.fileSystemAccessor.WriteAllText(contentFilePath, doContent.ToString());
        }

        private string GetEnvironmentContentFileName(string levelName)
        {
            return $"{levelName}.{DoFile.ContentFileNameExtension}";
        }

        private void BuildLabelsForLevel(DoFile doContent, QuestionnaireLevelLabels questionnaireLevelLabels)
        {
            foreach (var predefinedValue in questionnaireLevelLabels.PredefinedLabels)
            {
                doContent.AppendLine();

                string labelName = this.CreateLabelName(predefinedValue.LabelName);
                doContent.DefineLabel(labelName, predefinedValue.VariableValues);
            }

            foreach (var labeledVariable in questionnaireLevelLabels.LabeledVariable)
            {
                doContent.AppendLine();

                if (labeledVariable.Label.IsReference)
                {
                    string labelName = this.CreateLabelName(labeledVariable.Label.LabelName);
                    doContent.AssignValuesToVariable(labeledVariable.VariableName, labelName);
                }

                if (labeledVariable.Label.VariableValues.Any())
                {
                    string labelName = this.CreateLabelName(labeledVariable.VariableName);
                    doContent.DefineLabel(labelName, labeledVariable.Label.VariableValues);
                    doContent.AssignValuesToVariable(labeledVariable.VariableName, labelName);
                }

                doContent.AppendLabelToVariableMatching(labeledVariable.VariableName, labeledVariable.Label.LabelName);
            }
        }

        protected string CreateLabelName(string columnName)
        {
            return columnName;
        }
    }
}
