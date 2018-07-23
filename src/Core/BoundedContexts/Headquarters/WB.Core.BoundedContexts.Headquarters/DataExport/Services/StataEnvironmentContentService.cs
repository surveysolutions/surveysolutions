using System.Linq;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class StataEnvironmentContentService : IEnvironmentContentService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;
        private readonly InterviewActionsExporter interviewActionsExporter;
        private readonly CommentsExporter commentsExporter;
        private readonly IInterviewErrorsExporter interviewErrorsExporter;
        private readonly DiagnosticsExporter diagnosticsExporter;

        public StataEnvironmentContentService(
            IFileSystemAccessor fileSystemAccessor, 
            IQuestionnaireLabelFactory questionnaireLabelFactory,
            InterviewActionsExporter interviewActionsExporter,
            CommentsExporter commentsExporter,
            IInterviewErrorsExporter interviewErrorsExporter,
            DiagnosticsExporter diagnosticsExporter)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
            this.interviewActionsExporter = interviewActionsExporter;
            this.commentsExporter = commentsExporter;
            this.interviewErrorsExporter = interviewErrorsExporter;
            this.diagnosticsExporter = diagnosticsExporter;
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

            interviewActionsExporter.ExportActionsDoFile(folderPath);
            commentsExporter.ExportCommentsDoFile(questionnaireExportStructure, folderPath);
            interviewErrorsExporter.WriteDoFile(questionnaireExportStructure, folderPath);
            diagnosticsExporter.ExportDoFile(questionnaireExportStructure, folderPath);
        }

        private void CreateContentOfAdditionalFile(QuestionnaireLevelLabels questionnaireLevelLabels, string dataFileName, string basePath)
        {
            var doContent = new DoFile();

            doContent.BuildInsheet(dataFileName);

            this.BuildLabelsForLevel(doContent, questionnaireLevelLabels);

            var contentFilePath = this.fileSystemAccessor.CombinePath(basePath,
                this.GetEnvironmentContentFileName(questionnaireLevelLabels.LevelName));

            this.fileSystemAccessor.WriteAllText(contentFilePath, doContent.ToString());
        }

        private string GetEnvironmentContentFileName(string levelName)
        {
            return $"{levelName}.{DoFile.ContentFileNameExtension}";
        }

        protected void BuildLabelsForLevel(DoFile doContent, QuestionnaireLevelLabels questionnaireLevelLabels)
        {
            foreach (var variableLabel in questionnaireLevelLabels.LabeledVariable)
            {
                doContent.AppendLine();
                if (variableLabel.VariableValueLabels.Any())
                {
                    string labelName = this.CreateLabelName(variableLabel.VariableName);

                    doContent.AppendLabel(labelName, variableLabel.VariableValueLabels);

                    doContent.AppendLabelToValuesMatching(variableLabel.VariableName, labelName);
                }

                doContent.AppendLabelToVariableMatching(variableLabel.VariableName, variableLabel.Label);
            }
        }

        protected string CreateLabelName(string columnName)
        {
            return columnName;
        }
    }
}
