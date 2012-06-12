using System;
using Ncqrs;
using Ncqrs.Domain;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire;

namespace RavenQuestionnaire.Core.Domain
{
    /// <summary>
    /// Questionnaire Aggregate Root.
    /// </summary>
    public class QuestionnaireAR : AggregateRootMappedByConvention
    {
        private DateTime _creationDate;

        private QuestionnaireDocument _innerDocument = new QuestionnaireDocument();

        public QuestionnaireAR(){}
        
        public QuestionnaireAR(Guid questionnaireId, String text) : base(questionnaireId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewNoteAdded method).
            ApplyEvent(new NewQuestionnaireCreated
            {
                QuestionnaireId = questionnaireId,
                Title= text,
                CreationDate = clock.UtcNow()
            });
        }

        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewQuestionnaireCreated(NewQuestionnaireCreated e)
        {
            _innerDocument.Title = e.Title;
            _creationDate = e.CreationDate;
        }


        public void CreateCompletedQ(Guid completeQuestionnaireId)
        {
            CompleteQuestionnaireAR cq = new CompleteQuestionnaireAR(completeQuestionnaireId, _innerDocument);
        }


        public void AddGroup(string text, Propagate propagate, Guid? parentGroupKey)
        {



            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewNoteAdded method).
            ApplyEvent(new NewGroupAdded
            {
                GroupText = text,
                ParentGroupPublicKey = parentGroupKey,
                Paropagateble = propagate
            });
        }

        // Event handler for the NewGroupAdded event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewGroupAdded(NewGroupAdded e)
        {
            Group group = new Group();
            group.Title = e.GroupText;
            group.Propagated = e.Paropagateble;
            _innerDocument.Add(group, e.ParentGroupPublicKey);
        }
    }
}
