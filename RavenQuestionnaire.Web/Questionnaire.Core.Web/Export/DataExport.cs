// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataExport.cs" company="">
//   
// </copyright>
// <summary>
//   The data export.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace Questionnaire.Core.Web.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Export;
    using Main.Core.View;
    using Main.Core.View.Export;
    using Main.Core.View.StatusReport;

    using Ninject;
    using Ninject.Parameters;

    using SynchronizationMessages.Export;

    /// <summary>
    /// The data export.
    /// </summary>
    public class DataExport : IDataExport
    {
        #region Constants and Fields

        /// <summary>
        /// The kernel.
        /// </summary>
        private readonly IKernel kernel;

        /// <summary>
        /// The supplier.
        /// </summary>
        private readonly IEnvironmentSupplier<CompleteQuestionnaireExportView> supplier;

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataExport"/> class. 
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        public DataExport(IKernel kernel)
        {
            this.kernel = kernel;
            this.viewRepository = kernel.Get<IViewRepository>();
            this.supplier = kernel.Get<IEnvironmentSupplier<CompleteQuestionnaireExportView>>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The export data.
        /// </summary>
        /// <param name="templateGuid">
        /// The template guid.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// </returns>
        public byte[] ExportData(Guid templateGuid, string type)
        {
            if (type == "csv" || type == "tab")
            {
                string fileName = string.Format("exported{0}.zip", DateTime.Now.ToLongTimeString());
                var provider =
                    this.kernel.Get<IExportProvider<CompleteQuestionnaireExportView>>(
                        new ConstructorArgument("delimeter", type == "csv" ? ',' : '\t'));

                // new CSVExporter(type == "csv" ? ',' : '\t');
                var manager = new ExportManager<CompleteQuestionnaireExportView>(provider);
                var allLevels = new Dictionary<string, byte[]>();
                CQStatusReportView questionnairies =
                    this.viewRepository.Load<CQStatusReportViewInputModel, CQStatusReportView>(
                        new CQStatusReportViewInputModel(templateGuid, SurveyStatus.Approve.PublicId));
                this.CollectLevels(
                    new CompleteQuestionnaireExportInputModel(
                        questionnairies.Items.Select(q => q.PublicKey), templateGuid, null), 
                    allLevels, 
                    manager, 
                    null, 
                    type == "csv" ? FileType.Csv : FileType.Tab);
                this.supplier.AddCompletedResults(allLevels);
                return ZipFileData.ExportInternal(allLevels, fileName);
            }

            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The collect levels.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="manager">
        /// The manager.
        /// </param>
        /// <param name="parentName">
        /// The parent name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        protected void CollectLevels(
            CompleteQuestionnaireExportInputModel input,
            Dictionary<string, byte[]> container,
            ExportManager<CompleteQuestionnaireExportView> manager,
            string parentName,
            FileType type)
        {
            CompleteQuestionnaireExportView records =
                this.viewRepository.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>(input);

            if (records == null || !records.Items.Any())
                return;
            string fileName = this.GetName(records.GroupName, container, 0);
            container.Add(fileName, manager.ExportToStream(records));
            string currentName = this.supplier.BuildContent(records, parentName, fileName, type);
            foreach (Guid autoPropagatebleQuestionPublicKey in records.AutoPropagatebleQuestionsPublicKeys)
            {
                this.CollectLevels(
                    new CompleteQuestionnaireExportInputModel(input.QuestionnairiesForImport, input.TemplateId)
                        {
                            AutoPropagatebleQuestionPublicKey = autoPropagatebleQuestionPublicKey
                        },
                    container,
                    manager,
                    currentName,
                    type);
            }

            foreach (Guid subPropagatebleGroup in records.SubPropagatebleGroups)
            {
                this.CollectLevels(
                    new CompleteQuestionnaireExportInputModel(
                        input.QuestionnairiesForImport, input.TemplateId, subPropagatebleGroup),
                    container,
                    manager,
                    currentName,
                    type);
            }
        }

        /// <summary>
        /// The get name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="i">
        /// The i.
        /// </param>
        /// <returns>
        /// The get name.
        /// </returns>
        protected string GetName(string name, Dictionary<string, byte[]> container, int i)
        {
            if (i == 0)
            {
                if (!container.ContainsKey(name + ".csv"))
                {
                    return RemoveNonUnicode(name) + ".csv";
                }
                else
                {
                    return this.GetName(name, container, i + 1);
                }
            }

            if (!container.ContainsKey(name + i + ".csv"))
            {
                return RemoveNonUnicode(name) + i + ".csv";
            }
            else
            {
                return this.GetName(name, container, i + 1);
            }
        }
        protected string RemoveNonUnicode(string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
        }
        #endregion
    }
}