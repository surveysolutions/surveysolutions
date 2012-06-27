using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Domain
{
    /// <summary>
    /// CompleteQuestionnaire Aggregate Root.
    /// </summary>
    //[DynamicSnapshot]
    public class CompleteQuestionnaireAR : AggregateRootMappedByConvention, ISnapshotable<CompleteQuestionnaireDocument>
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
            var executor = new CompleteQuestionnaireConditionExecutor(new GroupHash(doc));
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
                QuestionnaireId = questionnaire.Id,
                Questionnaire = doc,
                CreationDate = clock.UtcNow(),
                Status = doc.Status,
                Responsible = doc.Responsible,
                TotalQuestionCount = doc.Find<ICompleteQuestion>(q=>!(q is IBinded)).Count()
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


        public void SetAnswer(Guid questionPublicKey, Guid? propogationPublicKey, object completeAnswer, List<object> completeAnswers)
        {
            //performe checka before event raising


            ICompleteQuestion question = FindQuestion(questionPublicKey, propogationPublicKey, _doc);
            // Apply a NewGroupAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnAnswerSet method).
            ApplyEvent(new AnswerSet
                           {
                               CompletedQuestionnaireId = this._doc.PublicKey,
                               Question = question,
                               Answer = completeAnswer ?? completeAnswers
                           });
        }

        // Event handler for the AnswerSet event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnAnswerSet(AnswerSet e)
        {
          
            
            e.Question.SetAnswer(e.Answer);
        }

        private static ICompleteQuestion FindQuestion(Guid questionKey, Guid? propagationKey, ICompleteGroup entity)
        {
            //PropagatableCompleteAnswer propagated = answer as PropagatableCompleteAnswer;

            var question = entity.FirstOrDefault<ICompleteQuestion>(q => q.PublicKey == questionKey);
            if (question == null)
                throw new ArgumentException("question wasn't found");
            if (!propagationKey.HasValue)
                return question;
            return entity.GetPropagatedQuestion(question.PublicKey, propagationKey.Value);
        }

        public void DeletePropagatableGroup(Guid propagationKey, Guid publicKey)
        {
            var group = new CompleteGroup(_doc.Find<CompleteGroup>(publicKey), propagationKey);
            ApplyEvent(new PropagatableGroupDeleted
                           {
                               CompletedQuestionnaireId = this._doc.PublicKey,
                               Group = group
                           });
        }

        // Event handler for the PropagatableGroupAdded event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnPropagatableGroupDeleted(PropagatableGroupDeleted e)
        {
            _doc.Remove(e.Group);
        }
        public void AddPropagatableGroup(Guid publicKey, Guid propagationKey)
        {
            //performe checka before event raising

            var template = _doc.Find<CompleteGroup>(publicKey);
            var executor = new CompleteQuestionnaireConditionExecutor(new GroupHash(_doc));
            executor.Execute(template);

            var newGroup = new CompleteGroup(template, propagationKey);

            // Apply a NewGroupAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnPropagatableGroupAdded method).
            ApplyEvent(new PropagatableGroupAdded
                           {
                               CompletedQuestionnaireId = this._doc.PublicKey,
                               Group = newGroup
                           });
        }

        // Event handler for the PropagatableGroupAdded event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnPropagatableGroupAdded(PropagatableGroupAdded e)
        {
           
            _doc.Add(e.Group, null);
        }

        #region Implementation of ISnapshotable<CompleteQuestionnaireDocument>

        public CompleteQuestionnaireDocument CreateSnapshot()
        {
            return this._doc;
        }

        public void RestoreFromSnapshot(CompleteQuestionnaireDocument snapshot)
        {
            this._doc = snapshot;
        }

        #endregion
    }
}
