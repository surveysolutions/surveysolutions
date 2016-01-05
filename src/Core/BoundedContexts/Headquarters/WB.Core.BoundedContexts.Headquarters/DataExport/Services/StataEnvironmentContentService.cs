using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class StataEnvironmentContentService : IEnvironmentContentService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQuestionnaireLabelFactory questionnaireLabelFactory;

        public StataEnvironmentContentService(
            IFileSystemAccessor fileSystemAccessor, 
            IQuestionnaireLabelFactory questionnaireLabelFactory)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireLabelFactory = questionnaireLabelFactory;
        }


        public void CreateEnvironmentFiles(QuestionnaireExportStructure questionnaireExportStructure, string folderPath, CancellationToken cancellationToken)
        {
            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                QuestionnaireLevelLabels questionnaireLevelLabels =
                      this.questionnaireLabelFactory.CreateLabelsForQuestionnaireLevel(questionnaireExportStructure, headerStructureForLevel.LevelScopeVector);

                this.CreateContentOfAdditionalFile(questionnaireLevelLabels,
                    ExportFileSettings.GetContentFileName(questionnaireLevelLabels.LevelName), folderPath);
            }
        }

        private void CreateContentOfAdditionalFile(QuestionnaireLevelLabels questionnaireLevelLabels, string dataFileName, string basePath)
        {
            var doContent = new StringBuilder();
            
            BuildInsheet(dataFileName, doContent);

            this.BuildLabelsForLevel(questionnaireLevelLabels, doContent);

            var contentFilePath = this.fileSystemAccessor.CombinePath(basePath,
                this.GetEnvironmentContentFileName(questionnaireLevelLabels.LevelName));

            this.fileSystemAccessor.WriteAllText(contentFilePath, doContent.ToString().ToLower());
        }

        private string GetEnvironmentContentFileName(string levelName)
        {
            return $"{levelName}.{this.ContentFileNameExtension}";
        }

        private string ContentFileNameExtension => "do";

        private static void BuildInsheet(string fileName, StringBuilder doContent)
        {
            doContent.AppendLine($"insheet using \"{fileName}\", tab");
        }

        protected void BuildLabelsForLevel(QuestionnaireLevelLabels questionnaireLevelLabels, StringBuilder doContent)
        {
            foreach (var variableLabel in questionnaireLevelLabels.LabeledVariable)
            {
                doContent.AppendLine();
                if (variableLabel.VariableValueLabels.Any())
                {
                    string labelName = this.CreateLabelName(variableLabel.VariableName);

                    this.AppendLabel(doContent, labelName, variableLabel.VariableValueLabels);
               
                    this.AppendLabelToValuesMatching(doContent, variableLabel.VariableName, labelName);
                }

                doContent.AppendLine($"label variable {variableLabel.VariableName} `\"{this.RemoveNotAllowedChars(variableLabel.Label)}\"'");
            }
        }

        private void AppendLabelToValuesMatching(StringBuilder doContent,string columnName, string labelName)
        {
            doContent.AppendLine($"label values {columnName} {labelName}");
        }

        private void AppendLabel(StringBuilder doContent, string labelName, IEnumerable<VariableValueLabel> labels)
        {
            //stata allows only int values less 2,147,483,620 to be labeled
            //stata doesn't allow to declare empty dictionaries
            int limitValue = 2147483620;
            var localBuilder = new StringBuilder();
            bool hasValidValue = false;

            foreach (var label in labels)
            {
                decimal value;
                if (decimal.TryParse(label.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value) && value < limitValue && (value % 1) == 0)
                {
                    localBuilder.Append($"{label.Value } `\"{this.RemoveNotAllowedChars(label.Label)}\"' ");
                    hasValidValue = true;
                }
                else
                    localBuilder.Append($"/*{label.Value} `\"{this.RemoveNotAllowedChars(label.Label)}\"'*/ ");
            }

            doContent.AppendFormat(hasValidValue ? "label define {0} " : "/*label define {0}*/ ", labelName);

            doContent.Append(localBuilder);

            doContent.AppendLine();
        }

        protected string CreateLabelName(string columnName)
        {
            return $"l{columnName}";
        }

        private string RemoveNotAllowedChars(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            //var onlyUnicode = Regex.Replace(s, @"[^\u0020-\u007E]", string.Empty);

            return Regex.Replace(s, @"\t|\n|\r|`|'", string.Empty);
        }
    }
}
