// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StataSuplier.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Export
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    using Main.Core.View.Export;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class StataSuplier : IEnvironmentSupplier<CompleteQuestionnaireExportView>
    {
        #region Fields

        /// <summary>
        /// The created labels.
        /// </summary>
        protected readonly List<Guid> createdLabels;

        /// <summary>
        /// The do content.
        /// </summary>
        protected readonly StringBuilder doContent;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StataSuplier"/> class.
        /// </summary>
        public StataSuplier()
        {
            this.doContent = new StringBuilder();
            this.createdLabels = new List<Guid>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add compled results.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public void AddCompletedResults(IDictionary<string, byte[]> container)
        {
            container.Add("data.do", this.CompileResult());
        }

        /// <summary>
        /// The build content.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <param name="parentTableName">
        /// The parent table name.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string BuildContent(
            CompleteQuestionnaireExportView result, string parentTableName, string fileName, FileType type)
        {
            // var primaryKeyColumnName = "PublicKey" + result.Parent.ToString();
            string primaryKeyColumnName = this.CreateColumnName(parentTableName, result.GroupName);
            this.BuildMerge(parentTableName, primaryKeyColumnName, fileName, type);
            this.BuildLabels(result.Header);

            this.SaveTempFile(primaryKeyColumnName);
            return primaryKeyColumnName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The build labels.
        /// </summary>
        /// <param name="header">
        /// The header.
        /// </param>
        protected void BuildLabels(HeaderCollection header)
        {
            foreach (HeaderItem headerItem in header)
            {
                if (headerItem.Labels.Count > 0)
                {
                    string labelName = this.CreateLabelName(headerItem);
                    if (!this.createdLabels.Contains(headerItem.PublicKey))
                    {
                        this.doContent.AppendLine();
                        this.doContent.AppendFormat(string.Format("label define {0} ", labelName));
                        foreach (var label in headerItem.Labels)
                        {
                            this.doContent.AppendFormat("{0} `\"{1}\"' ", label.Value.Caption, RemoveNonUnicode(label.Value.Title));
                        }

                        this.doContent.AppendLine();
                    }

                    this.doContent.AppendLine(string.Format("label values {0} {1}", headerItem.Caption, labelName));

                    this.createdLabels.Add(headerItem.PublicKey);
                }

                this.doContent.AppendLine(
                    string.Format("label var {0} `\"{1}\"'", headerItem.Caption, RemoveNonUnicode(headerItem.Title)));
            }
        }

        /// <summary>
        /// The build merge.
        /// </summary>
        /// <param name="parentPrimaryKeyName">
        /// The parent primary key name.
        /// </param>
        /// <param name="primaryKeyColumnName">
        /// The primary key column name.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        protected void BuildMerge(
            string parentPrimaryKeyName, string primaryKeyColumnName, string fileName, FileType type)
        {
            this.doContent.AppendLine("clear");
            this.doContent.AppendLine(
                string.Format("insheet using \"{0}\", {1}", fileName, type == FileType.Csv ? "comma" : "tab"));

            if (!string.IsNullOrEmpty(parentPrimaryKeyName))

                /*  {
                doContent.AppendLine("sort " + primaryKeyColumnName);
                doContent.AppendLine("tempfile ind");
                doContent.AppendLine("save \"`ind'\"");
            }
            else*/
            {
                this.doContent.AppendLine(string.Format("rename PublicKey {0}", primaryKeyColumnName));
                this.doContent.AppendLine(string.Format("rename ForeignKey {0}", parentPrimaryKeyName));

                this.doContent.AppendLine("sort " + parentPrimaryKeyName);

                this.doContent.AppendLine(string.Format("merge m:1 {0} using \"`{0}ind'\"", parentPrimaryKeyName));
                this.doContent.AppendLine("drop _merge");
            }
        }
       
        protected byte[] CompileResult()
        {
            this.doContent.AppendLine("list");
            
            return new UTF8Encoding().GetBytes(this.doContent.ToString().ToLower());
        }

        /// <summary>
        /// The create column name.
        /// </summary>
        /// <param name="parentTableName">
        /// The parent table name.
        /// </param>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected string CreateColumnName(string parentTableName, string tableName)
        {
            return string.IsNullOrEmpty(parentTableName)
                       ? "PublicKey"
                       : Regex.Replace(tableName, "[^_a-zA-Z0-9]", string.Empty);
        }

        /// <summary>
        /// The create label name.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected string CreateLabelName(HeaderItem item)
        {
            return string.Format("l{0}", item.Caption);
        }

        /// <summary>
        /// The save temp file.
        /// </summary>
        /// <param name="primaryKeyColumnName">
        /// The primary key column name.
        /// </param>
        protected void SaveTempFile(string primaryKeyColumnName)
        {
            string keyColumnName = "ind";
            if (primaryKeyColumnName.Length > symbolCount-keyColumnName.Length)
                primaryKeyColumnName = primaryKeyColumnName.Substring(0, symbolCount - keyColumnName.Length);
            primaryKeyColumnName += keyColumnName;
            this.doContent.AppendLine(string.Format("tempfile {0}", primaryKeyColumnName));
            this.doContent.AppendLine(string.Format("save \"`{0}'\"", primaryKeyColumnName));
        }

        private readonly int symbolCount = 31;

        protected string RemoveNonUnicode(string s)
        {
            var onlyUnicode = Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
            return Regex.Replace(onlyUnicode, @"\t|\n|\r", "");
        }

        #endregion
    }
}