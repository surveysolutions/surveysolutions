using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;

namespace RavenQuestionnaire.Core.Domain
{
    /// <summary>
    /// Questionnaire Aggregate Root.
    /// </summary>
    public class QuestionnaireAR : AggregateRootMappedByConvention, ISnapshotable<QuestionnaireDocument>
    {
        private DateTime _creationDate;

        private QuestionnaireDocument _innerDocument = new QuestionnaireDocument();

        public QuestionnaireAR(){}
        public QuestionnaireAR(QuestionnaireDocument template)
            : base(template.PublicKey)
        {
            ApplyEvent(new QuestionnaireTemplateLocaded
            {
                Template = template
            });
        }

        public QuestionnaireAR(Guid questionnaireId, String text) : base(questionnaireId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewQuestionnaireCreated method).
            ApplyEvent(new NewQuestionnaireCreated
            {
                PublicKey = questionnaireId,
                Title= text,
                CreationDate = clock.UtcNow()
            });


          /*  ApplyEvent(new QuestionnaireTemplateLocaded
            {
                Template = _innerDocument
            });*/
        }

        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewQuestionnaireCreated(NewQuestionnaireCreated e)
        {
            _innerDocument.Title = e.Title;
            _innerDocument.PublicKey = e.PublicKey;
            _creationDate = e.CreationDate;
        }

        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnQuestionnaireTemplateLocaded(QuestionnaireTemplateLocaded e)
        {
            _innerDocument = e.Template;
            _creationDate = e.Template.CreationDate;
        }

        public void CreateCompletedQ(Guid completeQuestionnaireId)
        {
            //TODO: check is it good to create new AR form another?
            CompleteQuestionnaireAR cq = new CompleteQuestionnaireAR(completeQuestionnaireId, _innerDocument);
        }


        public void AddGroup(Guid publicKey, string text, Propagate propagateble, Guid? parentGroupKey)
        {
            //performe checka before event raising


            // Apply a NewGroupAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewGroupAdded method).
            ApplyEvent(new NewGroupAdded
            {
                PublicKey = publicKey,
                GroupText = text,
                ParentGroupPublicKey = parentGroupKey,
                Paropagateble = propagateble
            });
        }

        

        // Event handler for the NewGroupAdded event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewGroupAdded(NewGroupAdded e)
        {
            Group group = new Group();
            group.Title = e.GroupText;
            group.Propagated = e.Paropagateble;
            group.PublicKey = e.PublicKey;

            _innerDocument.Add(group, e.ParentGroupPublicKey);
        }
        public void PreLoad()
        {
            ApplyEvent(new QuestionnaireLoaded());
        }
        protected void OnPreLoad(QuestionnaireLoaded e)
        {
        }
        public void DeleteQuestion(Guid questionId)
        {
            ApplyEvent(new QuestionDeleted() { QuestionId = questionId });
        }
        protected void OnQuestionDeleted(QuestionDeleted e)
        {
            this._innerDocument.Remove(e.QuestionId);
        }
        public void ChangeQuestion(Guid publicKey, string questionText, 
            string stataExportCaption, string instructions, 
            QuestionType questionType, Guid? groupPublicKey,
            string conditionExpression, string validationExpression,
            bool featured, Order answerOrder, Answer[] answers)
        {
            ApplyEvent(new QuestionChanged
            {
                QuestionText = questionText,
                StataExportCaption = stataExportCaption,
                QuestionType = questionType,
                ConditionExpression = conditionExpression,
                ValidationExpression = validationExpression,
                Featured = featured,
                AnswerOrder = answerOrder,
                PublicKey = publicKey,
                Answers = answers,
                Instructions = instructions
            });
        }

        /// <summary>
        /// Handler method for adding question.
        /// </summary>
        /// <param name="questionText"></param>
        /// <param name="stataExportCaption"></param>
        /// <param name="questionType"></param>
        /// <param name="conditionExpression"></param>
        /// <param name="validationExpression"></param>
        /// <param name="featured"></param>
        /// <param name="answerOrder"></param>
        /// <param name="instructions"> </param>
        /// <param name="groupPublicKey"></param>
        /// <param name="answers"></param>
        public void AddQuestion(Guid publicKey, string questionText, string stataExportCaption,QuestionType questionType,
                                                     string conditionExpression,string validationExpression, 
                                                     bool featured, Order answerOrder, string instructions,  Guid? groupPublicKey,
                                                     Answer[] answers)
        {
            //performe checks before event raising


            // Apply a NewQuestionAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewQuestionAdded method).
            ApplyEvent(new NewQuestionAdded
            {
                PublicKey = publicKey,
                QuestionText = questionText,
                StataExportCaption = stataExportCaption,
                QuestionType = questionType,
                ConditionExpression = conditionExpression,
                ValidationExpression = validationExpression,
                Featured = featured,
                AnswerOrder = answerOrder,
                GroupPublicKey = groupPublicKey,
                Answers = answers,
                Instructions = instructions
            });
        }

        // Event handler for the NewGroupAdded event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewQuestionAdded(NewQuestionAdded e)
        {
            var result = new CompleteQuestionFactory().Create(e.QuestionType);
            result.QuestionType = e.QuestionType;
            result.QuestionText = e.QuestionText;
            result.StataExportCaption = e.StataExportCaption;
            result.ConditionExpression = e.ConditionExpression;
            result.ValidationExpression = e.ValidationExpression;
            result.AnswerOrder = e.AnswerOrder;
            result.Featured = e.Featured;
            result.Instructions = e.Instructions;
            result.PublicKey = e.PublicKey;
            UpdateAnswerList(e.Answers, result);
            
            _innerDocument.Add(result, e.GroupPublicKey);
        }


        // Event handler for the QuestionChanged event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnQuestionChanged(QuestionChanged e)
        {

            var question = this._innerDocument.Find<AbstractQuestion>(e.PublicKey);
            if (question == null)
                return;
            question.QuestionText = e.QuestionText;
            question.StataExportCaption = e.StataExportCaption;
            question.QuestionType = e.QuestionType;

            UpdateAnswerList(e.Answers, question);

            question.ConditionExpression = e.ConditionExpression;
            question.ValidationExpression = e.ValidationExpression;
            question.Instructions = e.Instructions;
            question.Featured = e.Featured;
            question.AnswerOrder = e.AnswerOrder;
        }


        protected void UpdateAnswerList(IEnumerable<Answer> answers, AbstractQuestion question)
        {
            if (answers != null && answers.Any())
            {
                question.Children.Clear();
                foreach (Answer answer in answers)
                {
                    question.Add(answer, question.PublicKey);
                }
            }
        }

        #region Implementation of ISnapshotable<QuestionnaireDocument>

        public QuestionnaireDocument CreateSnapshot()
        {
            return this._innerDocument;
        }

        public void RestoreFromSnapshot(QuestionnaireDocument snapshot)
        {
            this._innerDocument = snapshot;
        }

        #endregion
    }
}
