// -----------------------------------------------------------------------
// <copyright file="TemplateExporter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Questionnaire.Core.Web.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Events;

    using Ncqrs.Restoring.EventStapshoot;

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
    public class TemplateExporter : ExportImportEvent, ITemplateExporter
    {
        #region Fields

        /// <summary>
        /// The synchronizer
        /// </summary>
        private readonly IEventSync synchronizer;

        #endregion
        
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateExporter"/> class.
        /// </summary>
        /// <param name="synchronizer">
        /// The synchronizer.
        /// </param>
        public TemplateExporter(IEventSync synchronizer)
                : base(synchronizer)
        {
            this.synchronizer = synchronizer;
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
            return this.ExportInternal(clientGuid, GetTemplate(templateGuid, clientGuid), string.Format("template{0}.txt", templateGuid == null ? "s" : string.Empty));
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
                    var payload = ((t.Payload as SnapshootLoaded).Template).Payload;
                    return payload != null && (payload as QuestionnaireDocument).PublicKey == templateGuid;
                }).FirstOrDefault());
            else
                archive = events;
            return archive;
        }

        #endregion
    }
}
