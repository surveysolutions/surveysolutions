using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class StataEnvironmentContentService : IEnvironmentContentService
    {
        public string BuildContentOfAdditionalFile(HeaderStructureForLevel headerStructureForLevel, string dataFileName)
        {
            var doContent = new StringBuilder();

            BuildInsheet(dataFileName, doContent);

            BuildLabelsForLevel(headerStructureForLevel, doContent);

            doContent.AppendLine("list");

            return doContent.ToString().ToLower();
        }

        public string GetEnvironmentContentFileName(string levelName)
        {
            return string.Format("{0}.do", levelName);
        }

        private static void BuildInsheet(string fileName, StringBuilder doContent)
        {
            doContent.AppendLine(
                string.Format("insheet using \"{0}\", comma", fileName));
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
                        string.Format("label variable {0} `\"{1}\"'", headerItem.ColumnNames[i], RemoveNonUnicode(headerItem.Titles[i])));
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
            doContent.AppendFormat("label define {0} ", labelName);
            foreach (var label in labels)
            {
                doContent.AppendFormat("{0} `\"{1}\"' ", label.Caption, this.RemoveNonUnicode(label.Title));
            }

            doContent.AppendLine();
        }

        protected string CreateLabelName(string columnName)
        {
            return string.Format("l{0}", columnName);
        }

        protected string RemoveNonUnicode(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            var onlyUnicode = Regex.Replace(s, @"[^\u0020-\u007E]", string.Empty);
            return Regex.Replace(onlyUnicode, @"\t|\n|\r", "");
        }

        protected string CreateColumnName(string parentTableName, string tableName)
        {
            return string.IsNullOrEmpty(parentTableName)
                       ? "PublicKey"
                       : Regex.Replace(tableName, "[^_a-zA-Z0-9]", string.Empty);
        }
    }
}
