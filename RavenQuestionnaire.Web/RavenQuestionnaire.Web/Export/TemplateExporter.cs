// -----------------------------------------------------------------------
// <copyright file="TemplateExporter.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Web.Mvc;
using Main.Core.View;
using Main.Core.View.Question;
using Ninject;
using Ninject.Parameters;
using RavenQuestionnaire.Core.Export;
using RavenQuestionnaire.Core.Export.csv;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export;
using RavenQuestionnaire.Core.Views.Group;

namespace RavenQuestionnaire.Web.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Events;

    using Ncqrs.Restoring.EventStapshoot;

    using Questionnaire.Core.Web.Export;

    /// <summary>
    /// Interface for exportTempaltes
    /// </summary>
    public interface ITemplateExporter
    {
        byte[] ExportTemplate(Guid? templateGuid, Guid? clientGuid);
        byte[] ExportData(Guid templateGuid, string type);
    }

    /// <summary>
    /// Class for exportTempaltes
    /// </summary>
    public class TemplateExporter : ZipExportImport, ITemplateExporter
    {
        #region Fields


        private readonly IViewRepository viewRepository;
        private readonly IKernel kernel;
        #endregion
        
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateExporter"/> class.
        /// </summary>
        /// <param name="synchronizer">
        /// The synchronizer.
        /// </param>
        public TemplateExporter(IKernel kernel)
            : base(kernel.Get<IEventSync>())
        {
            this.kernel = kernel;
            this.viewRepository = kernel.Get<IViewRepository>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gathe all questionnaireTemplates
        /// </summary>
        /// <param name="templateGuid">
        /// The template guid.
        /// </param>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        /// <returns>
        /// Zip-archive
        /// </returns>
        public byte[] ExportTemplate(Guid? templateGuid, Guid? clientGuid)
        {
            return this.ExportInternal(EventsToString(clientGuid, GetTemplate(templateGuid, clientGuid)),
                                       string.Format("template{0}.txt", templateGuid == null ? "s" : string.Empty));
        }

        public byte[] ExportData(Guid templateGuid, string type)
        {
            if (type == "csv" || type == "tab")
            {
                string fileName = string.Format("exported{0}.zip", DateTime.Now.ToLongTimeString());
                IExportProvider provider = kernel.Get<IExportProvider>(new ConstructorArgument("delimeter", type == "csv" ? ',' : '\t'));// new CSVExporter(type == "csv" ? ',' : '\t');
                var manager = new ExportManager(provider);
                var allLevels = new Dictionary<string, Stream>();
                CollectLevels(templateGuid, null, allLevels, manager);
                return this.ExportInternal(allLevels,
                                           fileName);
            }
            return null;
        }

        protected void CollectLevels(Guid templateGuid, Guid? level, Dictionary<string, Stream> container, ExportManager manager)
        {
            CompleteQuestionnaireExportView records =
                this.viewRepository.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>
                    (
                        new CompleteQuestionnaireExportInputModel
                            {
                                QuestionnaryId = templateGuid,
                                PropagatableGroupPublicKey = level
                            });
            container.Add("level" + level, manager.ExportToStream(records));
            foreach (Guid subPropagatebleGroup in records.SubPropagatebleGroups)
            {
                CollectLevels(templateGuid, subPropagatebleGroup, container, manager);
            }
        }

        /// <summary>
        /// Gathe all templates
        /// </summary>
        /// <param name="templateGuid">
        /// The template guid.
        /// </param>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        /// <returns>
        /// Zip-archive
        /// </returns>
        protected IEnumerable<AggregateRootEvent> GetTemplate(Guid? templateGuid, Guid? clientGuid)
        {
            var archive = new List<AggregateRootEvent>();
            var events = this.synchronizer.ReadEvents().ToList();
            if (templateGuid != null)
                archive.Add(events.Where(t =>
                {
                    var payload = ((t.Payload as SnapshootLoaded).Template).Payload as QuestionnaireDocument;
                    return payload != null && payload.PublicKey == templateGuid;
                }).FirstOrDefault());
            else 
                archive.AddRange(events.Where(ar=>ar.Payload is SnapshootLoaded).Where(aggregateRootEvent => ((SnapshootLoaded)(aggregateRootEvent.Payload)).Template.Payload is QuestionnaireDocument));
            return archive;
        }

        #endregion
    }
}
