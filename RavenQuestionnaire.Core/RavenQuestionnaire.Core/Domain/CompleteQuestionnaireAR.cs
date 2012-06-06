using System;
using Ncqrs;
using Ncqrs.Domain;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events;

namespace RavenQuestionnaire.Core.Domain
{
    /// <summary>
    /// CompleteQuestionnaire Aggregate Root.
    /// </summary>
    public class CompleteQuestionnaireAR : AggregateRootMappedByConvention
    {
        public CompleteQuestionnaireAR ()
        {
        }

        private CompleteQuestionnaireDocument _doc = new CompleteQuestionnaireDocument();
        private Guid _questionnaireId;
        private DateTime _creationDate;
     
        public CompleteQuestionnaireAR(Guid completeQuestionnaireId, string questionnaireId)
            : base(completeQuestionnaireId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewNoteAdded method).
            ApplyEvent(new NewCompleteQuestionnaireCreated
            {
                CompletedQuestionnaireId = completeQuestionnaireId,
                QuestionnaireIdOld = questionnaireId,
                CreationDate = clock.UtcNow()
            });
        }


        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewQuestionnaireCreated(NewCompleteQuestionnaireCreated e)
        {
            _questionnaireId = e.QuestionnaireId;
            _creationDate = e.CreationDate;
        }


    }
}
