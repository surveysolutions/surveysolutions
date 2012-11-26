// -----------------------------------------------------------------------
// <copyright file="IDataExport.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        }

        #endregion
        #region Fields

        private readonly IViewRepository viewRepository;
        private readonly IKernel kernel;
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
                        new CQStatusReportViewInputModel(templateGuid/*, SurveyStatus.Complete.PublicId*/));
                CollectLevels(new CompleteQuestionnaireExportInputModel(questionnairies.Items.Select(q => q.PublicKey), templateGuid, null), allLevels, manager);
                return this.ExportInternal(allLevels,
                                           fileName);
            }
            return null;
        }


        protected void CollectLevels(/*Guid templateId, */CompleteQuestionnaireExportInputModel input, Dictionary<string, byte[]> container, ExportManager<CompleteQuestionnaireExportView> manager)
        {
            CompleteQuestionnaireExportView records =
                this.viewRepository.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>
                    (input);
            container.Add(GetName(records.GroupName, container, 0), manager.ExportToStream(records));

            foreach (Guid autoPropagatebleQuestionPublicKey in records.AutoPropagatebleQuestionsPublicKeys)
            {

                CollectLevels(new CompleteQuestionnaireExportInputModel(input.QuestionnairiesForImport, input.TemplateId) { AutoPropagatebleQuestionPublicKey = autoPropagatebleQuestionPublicKey }, container,
                                  manager);
            }
            foreach (Guid subPropagatebleGroup in records.SubPropagatebleGroups)
            {
                CollectLevels(new CompleteQuestionnaireExportInputModel(input.QuestionnairiesForImport, input.TemplateId, subPropagatebleGroup), container, manager);
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
