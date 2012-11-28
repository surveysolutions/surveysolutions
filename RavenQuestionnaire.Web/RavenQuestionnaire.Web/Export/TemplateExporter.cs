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
        
    }

    /// <summary>
    /// Class for exportTempaltes
    /// </summary>
    public class TemplateExporter : ZipExportImport, ITemplateExporter
    {
      
        
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateExporter"/> class.
        /// </summary>
        /// <param name="synchronizer">
        /// The synchronizer.
        /// </param>
        public TemplateExporter(IEventSync sync)
            : base(sync)
        {
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
