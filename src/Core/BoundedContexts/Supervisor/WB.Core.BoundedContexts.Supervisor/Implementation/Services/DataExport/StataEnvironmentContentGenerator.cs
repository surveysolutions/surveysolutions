using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class StataEnvironmentContentGenerator
    {
        private readonly HeaderStructureForLevel headerStructureForLevel;
        private readonly string levelFileName;
        private readonly string levelFileNameWithExtension;
        private readonly FileType levelFileType;

        public StataEnvironmentContentGenerator(HeaderStructureForLevel headerStructureForLevel, string levelFileName, FileType levelFileType, string levelFileNameWithExtension)
        {
            this.headerStructureForLevel = headerStructureForLevel;
            this.levelFileName = levelFileName;
            this.levelFileType = levelFileType;
            this.levelFileNameWithExtension = levelFileNameWithExtension;
        }

        public string NameOfAdditionalFile
        {
            get { return string.Format("{0}.do", levelFileName); }
        }

        public byte[] ContentOfAdditionalFile
        {
            get
            {
                var doContent = new StringBuilder();

                BuildInsheet(levelFileNameWithExtension, levelFileType, doContent);

                BuildLabelsForLevel(headerStructureForLevel, doContent);

                doContent.AppendLine("list");

                return new UTF8Encoding().GetBytes(doContent.ToString().ToLower());
            }
        }

        private static void BuildInsheet(string fileName, FileType type, StringBuilder doContent)
        {
            doContent.AppendLine(
                string.Format("insheet using \"{0}\", {1}", fileName, type == FileType.Csv ? "comma" : "tab"));
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
