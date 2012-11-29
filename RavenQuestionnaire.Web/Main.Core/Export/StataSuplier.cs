// -----------------------------------------------------------------------
// <copyright file="Stata.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Main.Core.View.Export;

namespace Main.Core.Export
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class StataSuplier : IEnvironmentSupplier<CompleteQuestionnaireExportView>
    {
        protected readonly StringBuilder doContent;

        public StataSuplier()
        {
            doContent = new StringBuilder();
        }

        #region Implementation of IEnvironmentSupplier<CompleteQuestionnaireExportInputModel>

        public string BuildContent(CompleteQuestionnaireExportView result, string parentTableName, string fileName)
        {
            var primaryKeyColumnName = "PublicKey" + result.Parent.ToString();
            this.BuildMerge(parentTableName, primaryKeyColumnName, fileName);
            this.BuildLabels(result.Header);
            return primaryKeyColumnName;
        }
        protected void BuildMerge(string parentPrimaryKeyName, string primaryKeyColumnName, string fileName)
        {
            doContent.AppendLine("clear");
            doContent.AppendLine(string.Format("insheet using \"{0}\", comma", fileName));

            if (string.IsNullOrEmpty(parentPrimaryKeyName))
            {
                doContent.AppendLine("sort " + primaryKeyColumnName);
                doContent.AppendLine("tempfile ind");
                doContent.AppendLine("save \"`ind'\"");
            }
            else
            {
                doContent.AppendLine(string.Format("gen {0}=string(PublicKey)", primaryKeyColumnName));
                doContent.AppendLine("drop PublicKey");

                doContent.AppendLine(string.Format("gen {0}=string(ForeignKey)", parentPrimaryKeyName));
                doContent.AppendLine("drop ForeignKey");
                doContent.AppendLine("sort " + parentPrimaryKeyName);
                doContent.AppendLine(string.Format("merge m:1 {0} using \"`ind'\"", parentPrimaryKeyName));
                doContent.AppendLine("drop _merge");

            }
        }
        protected void BuildLabels(IDictionary<Guid,HeaderItem> header)
        {
            foreach (KeyValuePair<Guid, HeaderItem> headerItem in header)
            {
                if(headerItem.Value.Labels.Count==0)
                    continue;
                doContent.AppendLine();
                doContent.AppendFormat(string.Format("label define {0} ", headerItem.Key));
                foreach (var label in headerItem.Value.Labels)
                {

                    doContent.AppendFormat("{0} \"{1}\" ", label.Value.Caption, label.Value.Title);
                }
                doContent.AppendLine();
                doContent.AppendLine(string.Format("label var {0} {1}", headerItem.Value.Caption, headerItem.Key));
            }
        }

        public void AddCompledResults(IDictionary<string, byte[]> container)
        {
            container.Add("data.do",CompileResult());
        }

        protected byte[] CompileResult()
        {
            doContent.AppendLine("list");
            return  new ASCIIEncoding().GetBytes(doContent.ToString());
        }

       

        #endregion
    }
}
