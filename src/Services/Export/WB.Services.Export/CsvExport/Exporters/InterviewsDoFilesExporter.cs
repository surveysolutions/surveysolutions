using System.IO;
using System.Linq;
using System.Threading;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class InterviewsDoFilesExporter : IInterviewsDoFilesExporter
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;

        public InterviewsDoFilesExporter(IFileSystemAccessor fileSystemAccessor, IQuestionnaireLabelFactory questionnaireLabelFactory)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
        }

        public void ExportDoFiles(QuestionnaireExportStructure questionnaireExportStructure, string folderPath, in CancellationToken cancellationToken)
        {
            var questionnaireLabels = this.questionnaireLabelFactory.CreateLabelsForQuestionnaire(questionnaireExportStructure);

            foreach (var questionnaireLevelLabels in questionnaireLabels)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileName = ExportFileSettings.GetContentFileName(questionnaireLevelLabels.LevelName);
                this.CreateContentOfAdditionalFile(questionnaireLevelLabels, fileName, folderPath);
            }
        }


        private void CreateContentOfAdditionalFile(QuestionnaireLevelLabels questionnaireLevelLabels, string dataFileName, string basePath)
        {
            var doContent = new DoFile();

            doContent.BuildInsheet(dataFileName);

            this.BuildLabelsForLevel(doContent, questionnaireLevelLabels);

            var contentFilePath = Path.Combine(basePath, this.GetEnvironmentContentFileName(questionnaireLevelLabels.LevelName));
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

                string labelName = this.CreateLabelName(predefinedValue.Name);
                doContent.DefineLabel(labelName, predefinedValue.VariableValues);
            }

            foreach (var labeledVariable in questionnaireLevelLabels.LabeledVariable)
            {
                doContent.AppendLine();

                if (labeledVariable.Value.IsReference)
                {
                    string labelName = this.CreateLabelName(labeledVariable.Value.Name);
                    doContent.AssignValuesToVariable(labeledVariable.VariableName, labelName);
                }
                else if (labeledVariable.Value.VariableValues.Any())
                {
                    string labelName = this.CreateLabelName(labeledVariable.VariableName);
                    doContent.DefineLabel(labelName, labeledVariable.Value.VariableValues);
                    doContent.AssignValuesToVariable(labeledVariable.VariableName, labelName);
                }

                doContent.AppendLabelToVariableMatching(labeledVariable.VariableName, labeledVariable.VariableLabel);
            }
        }

        protected string CreateLabelName(string columnName)
        {
            return columnName;
        }
    }

    public interface IInterviewsDoFilesExporter
    {
        void ExportDoFiles(QuestionnaireExportStructure questionnaireExportStructure, string folderPath, in CancellationToken cancellationToken);
    }
}
