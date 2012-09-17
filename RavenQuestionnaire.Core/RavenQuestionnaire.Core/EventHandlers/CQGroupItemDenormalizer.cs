// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CQGroupItemDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The cq group item denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using Main.Core.Documents;
using Ncqrs.Restoring.EventStapshoot;

namespace RavenQuestionnaire.Core.EventHandlers
{
    using System.Linq;

    using Main.Core.Events.Questionnaire;
    using Main.Core.Events.Questionnaire.Completed;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using RavenQuestionnaire.Core.Denormalizers;
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
            HandleCompleteQuestionnaire(evnt.Payload.Questionnaire.TemplateId);
        }
        protected void HandleCompleteQuestionnaire(Guid templateId)
        {
            IQueryable<CQGroupItem> group =
               this.documentGroupSession.Query().Where(g => g.SurveyId == templateId);
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
           HandleQuestionnaire(evnt.Payload.Title, evnt.Payload.PublicKey);
        }
        protected void HandleQuestionnaire(string title, Guid key)
        {
            var questionnaire = new CQGroupItem(0, 100, 0, title, key);
            this.documentGroupSession.Store(questionnaire, key);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var questionnaireDoc = evnt.Payload.Template.Payload as QuestionnaireDocument;
            if (questionnaireDoc != null)
            {
                HandleQuestionnaire(questionnaireDoc.Title, questionnaireDoc.PublicKey);
                return;
            }
            var document = evnt.Payload.Template.Payload as CompleteQuestionnaireDocument;
            if (document != null)
            {
                HandleCompleteQuestionnaire(document.TemplateId);
            }
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