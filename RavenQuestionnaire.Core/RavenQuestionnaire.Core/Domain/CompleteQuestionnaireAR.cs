using System;
using System.Linq;
using Ncqrs;
using Ncqrs.Domain;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Domain
{
    /// <summary>
    /// CompleteQuestionnaire Aggregate Root.
    /// </summary>
    //[DynamicSnapshot]
    public class CompleteQuestionnaireAR : AggregateRootMappedByConvention
    {
        public CompleteQuestionnaireAR ()
        {
        }

        private CompleteQuestionnaireDocument _doc = new CompleteQuestionnaireDocument();
        private string _questionnaireId;
        private DateTime _creationDate;

        public CompleteQuestionnaireAR(Guid completeQuestionnaireId, QuestionnaireDocument questionnaire)
            : base(completeQuestionnaireId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            //check
            
            CompleteQuestionnaireDocument doc = (CompleteQuestionnaireDocument)questionnaire;

            doc.PublicKey = completeQuestionnaireId;
            
            ////Fix this with read model??
            doc.Creator = null; 
            doc.Status = new SurveyStatus();
            doc.Responsible = null;
            

            var questions = doc.GetAllQuestions<ICompleteQuestion>().ToList();
            var executor = new CompleteQuestionnaireConditionExecutor(doc);
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                if (completeQuestion is IBinded)
                    continue;
                completeQuestion.Enabled = executor.Execute(completeQuestion);
            }

            //ISubscriber ????

            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewNoteAdded method).
            ApplyEvent(new NewCompleteQuestionnaireCreated
            {
                CompletedQuestionnaireId = completeQuestionnaireId,
                Questionnaire = doc,
                CreationDate = clock.UtcNow()
            });
        }


        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewQuestionnaireCreated(NewCompleteQuestionnaireCreated e)
        {
            _questionnaireId = e.QuestionnaireId;
            _creationDate = e.CreationDate;
            _doc = e.Questionnaire;
        }

    }
}
