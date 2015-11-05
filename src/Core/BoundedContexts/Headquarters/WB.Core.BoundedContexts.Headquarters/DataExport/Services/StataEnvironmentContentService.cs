using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class StataEnvironmentContentService : IEnvironmentContentService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public StataEnvironmentContentService(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }


        public void CreateEnvironmentFiles(QuestionnaireExportStructure questionnaireExportStructure, string folderPath)
        {
            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                this.CreateContentOfAdditionalFile(headerStructureForLevel,
                    ExportFileSettings.GetContentFileName(headerStructureForLevel.LevelName), folderPath);
            }
        }

        public void CreateContentOfAdditionalFile(HeaderStructureForLevel headerStructureForLevel, string dataFileName, string basePath)
        {
            var doContent = new StringBuilder();
            
            BuildInsheet(dataFileName, doContent);

            this.BuildLabelsForLevel(headerStructureForLevel, doContent);

            var contentFilePath = this.fileSystemAccessor.CombinePath(basePath,
                this.GetEnvironmentContentFileName(headerStructureForLevel.LevelName));

            this.fileSystemAccessor.WriteAllText(contentFilePath, doContent.ToString().ToLower());
        }

        private string GetEnvironmentContentFileName(string levelName)
        {
            return string.Format("{0}.{1}", levelName, this.ContentFileNameExtension);
        }

        private string ContentFileNameExtension { get { return "do"; } }

        private static void BuildInsheet(string fileName, StringBuilder doContent)
        {
            doContent.AppendLine(
                string.Format("insheet using \"{0}\", tab", fileName));
        }

        protected void BuildLabelsForLevel(HeaderStructureForLevel headerStructureForLevel, StringBuilder doContent)
        {
            foreach (ExportedHeaderItem headerItem in headerStructureForLevel.HeaderItems.Values)
            {
                bool hasLabels = headerItem.Labels.Count > 0;

                string labelName = this.CreateLabelName(headerItem.VariableName);

                doContent.AppendLine();
                
                if (hasLabels)
                {
                    this.AppendLabel(doContent, labelName, headerItem.Labels.Values);
                }

                for (int i = 0; i < headerItem.ColumnNames.Length; i++)
                {
                    if (hasLabels)
                    {
                        this.AppendLabelToValuesMatching(doContent, headerItem.ColumnNames[i], labelName);
                    }

                    doContent.AppendLine(
                        string.Format("label variable {0} `\"{1}\"'", headerItem.ColumnNames[i], this.RemoveNotAllowedChars(headerItem.Titles[i])));
                }
            }

            if (headerStructureForLevel.LevelLabels != null)
            {
                var levelLabelName = this.CreateLabelName(headerStructureForLevel.LevelIdColumnName);
                this.AppendLabel(doContent, levelLabelName, headerStructureForLevel.LevelLabels);
                this.AppendLabelToValuesMatching(doContent, headerStructureForLevel.LevelIdColumnName, levelLabelName);
            }

        }

        private void AppendLabelToValuesMatching(StringBuilder doContent,string columnName, string labelName)
        {
            doContent.AppendLine(string.Format("label values {0} {1}", columnName, labelName));
        }

        private void AppendLabel(StringBuilder doContent, string labelName, IEnumerable<LabelItem> labels)
        {
            //stata allows only int values less 2,147,483,620 to be labeled
            //stata doesn't allow to declare empty dictionaries
            int limitValue = 2147483620;
            var localBuilder = new StringBuilder();
            bool hasValidValue = false;

            foreach (var label in labels)
            {
                int value;
                if (int.TryParse(label.Caption, out value) && value < limitValue)
                {
                    localBuilder.AppendFormat("{0} `\"{1}\"' ", label.Caption, this.RemoveNotAllowedChars(label.Title));
                    hasValidValue = true;
                }
                else
                    localBuilder.AppendFormat("/*{0} `\"{1}\"'*/ ", label.Caption, this.RemoveNotAllowedChars(label.Title));
            }

            doContent.AppendFormat(hasValidValue ? "label define {0} " : "/*label define {0}*/ ", labelName);

            doContent.Append(localBuilder);

            doContent.AppendLine();
        }

        protected string CreateLabelName(string columnName)
        {
            return string.Format("l{0}", columnName);
        }

        private string RemoveNotAllowedChars(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            //var onlyUnicode = Regex.Replace(s, @"[^\u0020-\u007E]", string.Empty);

            return Regex.Replace(s, @"\t|\n|\r|`|'|""", string.Empty);
        }
    }
}
