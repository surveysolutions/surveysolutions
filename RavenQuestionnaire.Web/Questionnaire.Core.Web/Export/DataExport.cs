// -----------------------------------------------------------------------
// <copyright file="IDataExport.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.Core.Export;
using Main.Core.View;
using Main.Core.View.Export;
using Main.Core.View.StatusReport;
using Ninject;
using Ninject.Parameters;

namespace Questionnaire.Core.Web.Export
{
    public class DataExport : ZipExportImport,IDataExport
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateExporter"/> class.
        /// </summary>
        /// <param name="synchronizer">
        /// The synchronizer.
        /// </param>
        public DataExport(IKernel kernel)
            : base(kernel.Get<IEventSync>())
        {
            this.kernel = kernel;
            this.viewRepository = kernel.Get<IViewRepository>();
            this.supplier = kernel.Get<IEnvironmentSupplier<CompleteQuestionnaireExportView>>();
        }

        #endregion
        #region Fields

        private readonly IViewRepository viewRepository;
        private readonly IKernel kernel;
        private readonly IEnvironmentSupplier<CompleteQuestionnaireExportView> supplier;
        #endregion
        public byte[] ExportData(Guid templateGuid, string type)
        {
            if (type == "csv" || type == "tab")
            {
                string fileName = string.Format("exported{0}.zip", DateTime.Now.ToLongTimeString());
                var provider = kernel.Get<IExportProvider<CompleteQuestionnaireExportView>>(new ConstructorArgument("delimeter", type == "csv" ? ',' : '\t'));// new CSVExporter(type == "csv" ? ',' : '\t');
                var manager = new ExportManager<CompleteQuestionnaireExportView>(provider);
                var allLevels = new Dictionary<string, byte[]>();
                var questionnairies =
                    this.viewRepository.Load<CQStatusReportViewInputModel, CQStatusReportView>(
                        new CQStatusReportViewInputModel(templateGuid, SurveyStatus.Approve.PublicId));
                CollectLevels(
                    new CompleteQuestionnaireExportInputModel(questionnairies.Items.Select(q => q.PublicKey),
                                                              templateGuid, null), allLevels, manager, null,
                    type == "csv" ? FileType.Csv : FileType.Tab);
                this.supplier.AddCompledResults(allLevels);
                return this.ExportInternal(allLevels,
                                           fileName);
            }
            return null;
        }


        protected void CollectLevels(CompleteQuestionnaireExportInputModel input, Dictionary<string, byte[]> container, ExportManager<CompleteQuestionnaireExportView> manager, string parentName,FileType type)
        {
            CompleteQuestionnaireExportView records =
                this.viewRepository.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>
                    (input);
            if(records==null)
                return;
            var fileName = GetName(records.GroupName, container, 0);
            container.Add(fileName, manager.ExportToStream(records));
            var currentName = this.supplier.BuildContent(records, parentName, fileName, type);
            foreach (Guid autoPropagatebleQuestionPublicKey in records.AutoPropagatebleQuestionsPublicKeys)
            {

                CollectLevels(
                    new CompleteQuestionnaireExportInputModel(input.QuestionnairiesForImport, input.TemplateId)
                        {AutoPropagatebleQuestionPublicKey = autoPropagatebleQuestionPublicKey}, container,
                    manager, currentName, type);
            }
            foreach (Guid subPropagatebleGroup in records.SubPropagatebleGroups)
            {
                CollectLevels(
                    new CompleteQuestionnaireExportInputModel(input.QuestionnairiesForImport, input.TemplateId,
                                                              subPropagatebleGroup), container, manager, currentName, type);
            }

        }

        protected string GetName(string name, Dictionary<string, byte[]> container, int i)
        {
            if (i == 0)
            {
                if (!container.ContainsKey(name + ".csv"))
                    return name + ".csv";
                else
                    return GetName(name, container, i + 1);
            }
            if (!container.ContainsKey(name + i + ".csv"))
                return name + i + ".csv";
            else
                return GetName(name, container, i + 1);
        }
    }
}
