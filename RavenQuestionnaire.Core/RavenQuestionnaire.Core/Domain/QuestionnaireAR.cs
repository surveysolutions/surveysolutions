using Ncqrs;
using System;
using System.Linq;
using Ncqrs.Domain;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.SubEntities;
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
        
        public void AddGroup(Guid publicKey, string text, Propagate propagateble, Guid? parentGroupKey, string conditionExpression)
        {
            //performe checka before event raising


            // Apply a NewGroupAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewGroupAdded method).
            ApplyEvent(new NewGroupAdded
            {
                QuestionnairePublicKey=this._innerDocument.PublicKey,
                PublicKey = publicKey,
                GroupText = text,
                ParentGroupPublicKey = parentGroupKey,
                Paropagateble = propagateble,
                ConditionExpression = conditionExpression
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
            group.ConditionExpression = e.ConditionExpression;
            _innerDocument.Add(group, e.ParentGroupPublicKey);
        }

        public void PreLoad()
        {
            ApplyEvent(new QuestionnaireLoaded());
        }

        protected void OnPreLoad(QuestionnaireLoaded e)
        {
        }

        public void MoveQuestionnaireItem(Guid publicKey, Guid? groupKey, Guid? afterItemKey)
        {
            ApplyEvent(new QuestionnaireItemMoved
            {
                AfterItemKey = afterItemKey,
                GroupKey = groupKey,
                PublicKey = publicKey
            });
        }

        protected void OnQuestionnaireItemMoved(QuestionnaireItemMoved e)
        {
            var questionnaire = new Questionnaire(this._innerDocument);
            questionnaire.MoveItem(e.PublicKey, e.GroupKey, e.AfterItemKey);
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
            string conditionExpression, string validationExpression, string validationMessage,
            bool featured, bool mandatory, Order answerOrder, Answer[] answers)
        {
            ApplyEvent(new QuestionChanged
            {
                QuestionText = questionText,
                StataExportCaption = stataExportCaption,
                QuestionType = questionType,
                ConditionExpression = conditionExpression,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Featured = featured,
                Mandatory = mandatory,
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
                                                     string conditionExpression,string validationExpression, string validationMessage,
                                                     bool featured, bool mandatory, Order answerOrder, string instructions,  Guid? groupPublicKey,
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
                ValidationMessage = validationMessage,
                Featured = featured,
                Mandatory = mandatory,
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
            result.ValidationMessage = e.ValidationMessage;
            result.AnswerOrder = e.AnswerOrder;
            result.Featured = e.Featured;
            result.Mandatory = e.Mandatory;
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
            question.ValidationMessage = e.ValidationMessage;
            question.Instructions = e.Instructions;
            question.Featured = e.Featured;
            question.Mandatory = e.Mandatory;
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

        public void UpdateImage(Guid questionKey, Guid imageKey, string title, string description)
        {
            ApplyEvent(new ImageUpdated() { Description = description, ImageKey = imageKey, QuestionKey = questionKey, Title = title });
        }

        protected void OnImageUpdated(ImageUpdated e)
        {
            var question = this._innerDocument.Find<AbstractQuestion>(e.QuestionKey);

            question.UpdateCard(e.ImageKey, e.Title, e.Description);
        }
        protected void OnImageUploaded(ImageUploaded e)
        {

            var newImage = new Image
            {
                PublicKey = e.ImagePublicKey,
                Title = e.Title,
                Description = e.Description,
                CreationDate = DateTime.Now
            };

            var question = this._innerDocument.Find<AbstractQuestion>(e.PublicKey);

            question.AddCard(newImage);
        }
        public void DeleteImage(Guid questionKey, Guid imageKey)
        {
            ApplyEvent(new ImageDeleted() {ImageKey = imageKey, QuestionKey = questionKey});
        }

        protected void OnImageDeleted(ImageDeleted e)
        {
            var question = this._innerDocument.Find<AbstractQuestion>(e.QuestionKey);

            question.RemoveCard(e.ImageKey);
        }

        public void DeleteGroup(Guid groupPublicKey)
        {
            ApplyEvent(new GroupDeleted(){ GroupPublicKey = groupPublicKey});
        }

        protected void OnGroupDeleted(GroupDeleted e)
        {
            this._innerDocument.Remove(e.GroupPublicKey);
        }

        //public void UpdateGroup(string groupText, Propagate propagateble, Guid groupPublicKey, List<Guid> triggers)
        //{
        //    Group group = this._innerDocument.Find<Group>(groupPublicKey);
        //    if (group == null)
        //        throw new ArgumentException(string.Format("group with  publick key {0} can't be found", groupPublicKey));
        //    ApplyEvent(new GroupUpdated()
        //                   {
        //                       parentGroup = groupPublicKey,
        //                       GroupText = groupText,
        //                       Propagateble = propagateble,
        //                       Triggers = triggers
        //                   });
        //}

        public void UpdateGroup(string groupText, Propagate propagateble, Guid groupPublicKey, UserLight executor, string conditionExpression)
        {
            Group group = this._innerDocument.Find<Group>(groupPublicKey);
            if (group == null)
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", groupPublicKey));
            ApplyEvent(new GroupUpdated()
            {
                GroupPublicKey = groupPublicKey,
                GroupText = groupText,
                Propagateble = propagateble,
                Executor = executor,
                ConditionExpression = conditionExpression
            });
        }


        protected void OnGroupUpdated(GroupUpdated e)
        {
            Group group = this._innerDocument.Find<Group>(e.GroupPublicKey);
            if (group != null)
            {
                group.Propagated = e.Propagateble;
                //if(e.Triggers!=null)
                //    group.Triggers = e.Triggers;
                group.ConditionExpression = e.ConditionExpression;
                group.Update(e.GroupText);
                return;
            }
        }

        public void UploadImage(Guid publicKey, string title, string description,
            Guid imagePublicKey)
        {
            ApplyEvent(new ImageUploaded()
                           {
                               Description = description,
                               Title = title,
                               PublicKey = publicKey,
                               ImagePublicKey = imagePublicKey
                           });
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
