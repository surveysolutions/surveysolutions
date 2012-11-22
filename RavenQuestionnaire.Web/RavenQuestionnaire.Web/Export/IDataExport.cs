// -----------------------------------------------------------------------
// <copyright file="IDataExport.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;
using Ninject;
using Ninject.Parameters;
using Questionnaire.Core.Web.Export;
using RavenQuestionnaire.Core.Export;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;
using RavenQuestionnaire.Core.Views.StatusReport;

namespace RavenQuestionnaire.Web.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IDataExport
    {
        byte[] ExportData(Guid templateGuid, string type);
    }

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
                var allLevels = new Dictionary<string, Stream>();
                var questionnairies =
                    this.viewRepository.Load<CQStatusReportViewInputModel, CQStatusReportView>(
                        new CQStatusReportViewInputModel(templateGuid/*, SurveyStatus.Complete.PublicId*/));
                CollectLevels(templateGuid, questionnairies.Items.Select(q=>q.PublicKey), null, allLevels, manager);
                return this.ExportInternal(allLevels,
                                           fileName);
            }
            return null;
        }

        protected void CollectLevels(Guid templateId, IEnumerable<Guid> questionnairiesGuids, Guid? level, Dictionary<string, Stream> container, ExportManager<CompleteQuestionnaireExportView> manager)
        {
            CompleteQuestionnaireExportView records =
                this.viewRepository.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>
                    (
                        new CompleteQuestionnaireExportInputModel(questionnairiesGuids, templateId, level));
            container.Add(records.GroupName+".csv", manager.ExportToStream(records));
            foreach (Guid subPropagatebleGroup in records.SubPropagatebleGroups)
            {
                CollectLevels(templateId,questionnairiesGuids, subPropagatebleGroup, container, manager);
            }
        }
    }
}
