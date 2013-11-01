using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Main.Core.Export;
using Main.Core.View.Export;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class StataEnvironmentSupplier : IEnvironmentSupplier<InterviewDataExportView>
    {
        private readonly Dictionary<string, StringBuilder> doFiles;

        public StataEnvironmentSupplier()
        {
            this.doFiles = new Dictionary<string, StringBuilder>();
        }

        public void AddCompletedResults(IDictionary<string, byte[]> container)
        {
            foreach (var doFile in doFiles)
            {
                var doContent = doFile.Value;
                doContent.AppendLine("list");
                var toBytes = new UTF8Encoding().GetBytes(doContent.ToString().ToLower());
                container.Add(string.Format("{0}.do", doFile.Key), toBytes);
            }
        }

        public string BuildContent(InterviewDataExportView result, string parentPrimaryKeyName, string fileName, FileType type)
        {
            string primaryKeyColumnName = CreateColumnName(parentPrimaryKeyName, result.LevelName);

            var doContent = new StringBuilder();

            BuildInsheet(fileName, type, doContent);

            BuildLabelsForLevel(result, doContent);

            doFiles.Add(result.LevelName, doContent);

            return primaryKeyColumnName;
        }

        private static void BuildInsheet(string fileName, FileType type, StringBuilder doContent)
        {
            doContent.AppendLine(
                string.Format("insheet using \"{0}\", {1}", fileName, type == FileType.Csv ? "comma" : "tab"));
        }

        protected void BuildLabelsForLevel(InterviewDataExportView result, StringBuilder doContent)
        {
            foreach (ExportedHeaderItem headerItem in result.Header)
            {
                bool hasLabels = headerItem.Labels.Count > 0;

                string labelName = this.CreateLabelName(headerItem.VariableName);

                doContent.AppendLine();
                
                if (hasLabels)
                {
                    doContent.AppendFormat("label define {0} ", labelName);
                    foreach (var label in headerItem.Labels)
                    {
                        doContent.AppendFormat("{0} `\"{1}\"' ", label.Value.Caption, RemoveNonUnicode(label.Value.Title));
                    }

                    doContent.AppendLine();
                }

                for (int i = 0; i < headerItem.ColumnNames.Length; i++)
                {
                    if (hasLabels)
                    {
                        if (headerItem.Labels.Count > 0)
                        {
                            doContent.AppendLine(string.Format("label values {0} {1}", headerItem.ColumnNames[i], labelName));
                        }
                    }

                    doContent.AppendLine(
                        string.Format("label variable {0} `\"{1}\"'", headerItem.ColumnNames[i], RemoveNonUnicode(headerItem.Titles[i])));
                }
            }
        
        }

        protected string CreateLabelName(string columnName)
        {
            return string.Format("l{0}", columnName);
        }

        protected string RemoveNonUnicode(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            var onlyUnicode = Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
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
