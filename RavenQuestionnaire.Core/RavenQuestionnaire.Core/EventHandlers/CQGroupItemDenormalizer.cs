// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CQGroupItemDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The cq group item denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Ncqrs.Restoring.EventStapshoot;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.EventHandlers
{
    using System.Linq;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Events;
    using RavenQuestionnaire.Core.Events.Questionnaire;
    using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

    /// <summary>
    /// The cq group item denormalizer.
    /// </summary>
    public class CQGroupItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>, 
                                           IEventHandler<NewQuestionnaireCreated>,
                                           IEventHandler<SnapshootLoaded>, 
                                           IEventHandler<CompleteQuestionnaireDeleted>
    {
        #region Fields

        /// <summary>
        /// The document group session.
        /// </summary>
        private readonly IDenormalizerStorage<CQGroupItem> documentGroupSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CQGroupItemDenormalizer"/> class.
        /// </summary>
        /// <param name="documentGroupSession">
        /// The document group session.
        /// </param>
        public CQGroupItemDenormalizer(IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentGroupSession = documentGroupSession;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            IQueryable<CQGroupItem> group =
                this.documentGroupSession.Query().Where(g => g.SurveyId == evnt.Payload.Questionnaire.TemplateId);
            foreach (CQGroupItem cqGroupItem in group)
            {
                cqGroupItem.TotalCount++;
            }
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            var questionnaire = new CQGroupItem(0, 100, 0, evnt.Payload.Title, evnt.Payload.PublicKey);
            this.documentGroupSession.Store(questionnaire, evnt.Payload.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as QuestionnaireDocument;
            if (document == null)
                return;
            var questionnaire = new CQGroupItem(
                0, 100, 0, document.Title, document.PublicKey);
            this.documentGroupSession.Store(questionnaire, document.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            IQueryable<CQGroupItem> group =
                this.documentGroupSession.Query().Where(g => g.SurveyId == evnt.Payload.TemplateId);
            foreach (CQGroupItem cqGroupItem in group)
            {
                cqGroupItem.TotalCount--;
            }
        }

        #endregion
    }
}