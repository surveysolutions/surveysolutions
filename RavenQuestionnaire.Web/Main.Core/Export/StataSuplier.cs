// -----------------------------------------------------------------------
// <copyright file="Stata.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Main.Core.View.Export;

namespace Main.Core.Export
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class StataSuplier : IEnvironmentSupplier<CompleteQuestionnaireExportView>
    {
        protected readonly StringBuilder doContent;
        protected readonly List<Guid> createdLabels;
        public StataSuplier()
        {
            doContent = new StringBuilder();
            createdLabels=new List<Guid>();
        }

        #region Implementation of IEnvironmentSupplier<CompleteQuestionnaireExportInputModel>

        public string BuildContent(CompleteQuestionnaireExportView result, string parentTableName, string fileName,  FileType type)
        {
            //var primaryKeyColumnName = "PublicKey" + result.Parent.ToString();
            var primaryKeyColumnName = CreateColumnName(parentTableName,result.GroupName);
            this.BuildMerge(parentTableName, primaryKeyColumnName, fileName, type);
            this.BuildLabels(result.Header);

            this.SaveTempFile(primaryKeyColumnName);
            return primaryKeyColumnName;
        }
        protected void SaveTempFile(string primaryKeyColumnName)
        {
            doContent.AppendLine(string.Format("tempfile {0}ind",primaryKeyColumnName));
            doContent.AppendLine(string.Format("save \"`{0}ind'\"", primaryKeyColumnName));
        }

        protected void BuildMerge(string parentPrimaryKeyName, string primaryKeyColumnName, string fileName,  FileType type)
        {
            doContent.AppendLine("clear");
            doContent.AppendLine(string.Format("insheet using \"{0}\", {1}", fileName, type == FileType.Csv ? "comma" : "tab"));

            if (!string.IsNullOrEmpty(parentPrimaryKeyName))
          /*  {
                doContent.AppendLine("sort " + primaryKeyColumnName);
                doContent.AppendLine("tempfile ind");
                doContent.AppendLine("save \"`ind'\"");
            }
            else*/
            {
                doContent.AppendLine(string.Format("rename PublicKey {0}", primaryKeyColumnName));
                doContent.AppendLine(string.Format("rename ForeignKey {0}", parentPrimaryKeyName));

                doContent.AppendLine("sort " + parentPrimaryKeyName);

                doContent.AppendLine(string.Format("merge m:1 {0} using \"`{0}ind'\"", parentPrimaryKeyName));
                doContent.AppendLine("drop _merge");

            }
        }
        protected void BuildLabels(HeaderCollection header)
        {
            foreach (var headerItem in header)
            {
                if (headerItem.Labels.Count > 0)
                {
                    var labelName = CreateLabelName(headerItem);
                    if (!createdLabels.Contains(headerItem.PublicKey))
                    {
                        doContent.AppendLine();
                        doContent.AppendFormat(string.Format("label define {0} ", labelName));
                        foreach (var label in headerItem.Labels)
                        {
                            doContent.AppendFormat("{0} `\"{1}\"' ", label.Value.Caption, label.Value.Title);
                        }
                        doContent.AppendLine();

                    }
                    doContent.AppendLine(string.Format("label values {0} {1}", headerItem.Caption, labelName));
                    


                    createdLabels.Add(headerItem.PublicKey);
                }
                doContent.AppendLine(string.Format("label var {0} `\"{1}\"'", headerItem.Caption, headerItem.Title));
            }
        }

        public void AddCompledResults(IDictionary<string, byte[]> container)
        {
            container.Add("data.do",CompileResult());
        }

        protected byte[] CompileResult()
        {
            doContent.AppendLine("list");
            return  new ASCIIEncoding().GetBytes(doContent.ToString().ToLower());
        }

       

        #endregion
        protected string CreateLabelName(HeaderItem item)
        {
            return string.Format("l{0}", item.Caption);
        }
        protected string CreateColumnName(string parentTableName, string tableName)
        {

            return string.IsNullOrEmpty(parentTableName) ? "PublicKey" : Regex.Replace(tableName, "[^_a-zA-Z0-9]", string.Empty);
        }
    }
}
