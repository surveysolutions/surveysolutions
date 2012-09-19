// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireAR.cs" company="">
//   
// </copyright>
// <summary>
//   Questionnaire Aggregate Root.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Ncqrs.Restoring.EventStapshoot;

namespace Main.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Questionnaire;

    using Ncqrs;
    using Ncqrs.Domain;
    using Ncqrs.Eventing.Sourcing.Snapshotting;

    /// <summary>
    /// Questionnaire Aggregate Root.
    /// </summary>
    public class QuestionnaireAR : SnapshootableAggregateRoot<QuestionnaireDocument>
    {
        #region Fields

        /// <summary>
        /// The _inner document.
        /// </summary>
        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireAR"/> class.
        /// </summary>
        public QuestionnaireAR()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireAR"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public QuestionnaireAR(Guid questionnaireId, string text)
            : base(questionnaireId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewQuestionnaireCreated method).
            this.ApplyEvent(
                new NewQuestionnaireCreated { PublicKey = questionnaireId, Title = text, CreationDate = clock.UtcNow() });
        }

        #endregion

        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.
        #region Public Methods and Operators

        /// <summary>
        /// The add group.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="propagateble">
        /// The propagateble.
        /// </param>
        /// <param name="parentGroupKey">
        /// The parent group key.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        public void AddGroup(
            Guid publicKey, string text, Propagate propagateble, Guid? parentGroupKey, string conditionExpression)
        {
            //// performe checka before event raising

            // Apply a NewGroupAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewGroupAdded method).
            this.ApplyEvent(
                new NewGroupAdded
                    {
                        QuestionnairePublicKey = this.innerDocument.PublicKey, 
                        PublicKey = publicKey, 
                        GroupText = text, 
                        ParentGroupPublicKey = parentGroupKey, 
                        Paropagateble = propagateble, 
                        ConditionExpression = conditionExpression
                    });
        }

        // Event handler for the NewGroupAdded event. This method
        // is automaticly wired as event handler based on convension.

        /// <summary>
        /// Handler method for adding question.
        /// </summary>
        /// <param name="publicKey">
        /// The public Key.
        /// </param>
        /// <param name="questionText">
        /// </param>
        /// <param name="stataExportCaption">
        /// </param>
        /// <param name="questionType">
        /// </param>
        /// <param name="conditionExpression">
        /// </param>
        /// <param name="validationExpression">
        /// </param>
        /// <param name="validationMessage">
        /// The validation Message.
        /// </param>
        /// <param name="featured">
        /// </param>
        /// <param name="mandatory">
        /// The mandatory.
        /// </param>
        /// <param name="answerOrder">
        /// </param>
        /// <param name="instructions">
        /// </param>
        /// <param name="groupPublicKey">
        /// </param>
        /// <param name="targetGroupKey">
        /// The Target Group Key.
        /// </param>
        /// <param name="answers">
        /// </param>
        public void AddQuestion(
            Guid publicKey, 
            string questionText, 
            string stataExportCaption, 
            QuestionType questionType, 
            string conditionExpression, 
            string validationExpression, 
            string validationMessage, 
            bool featured, 
            bool mandatory, 
            Order answerOrder, 
            string instructions, 
            Guid? groupPublicKey, 
            Guid targetGroupKey, 
            Answer[] answers)
        {
            //// performe checks before event raising

            // Apply a NewQuestionAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewQuestionAdded method).
            this.ApplyEvent(
                new NewQuestionAdded
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
                        TargetGroupKey = targetGroupKey, 
                        Answers = answers, 
                        Instructions = instructions
                    });
        }

        /// <summary>
        /// The change question.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionText">
        /// The question text.
        /// </param>
        /// <param name="targetGroupKey">
        /// The target group key.
        /// </param>
        /// <param name="stataExportCaption">
        /// The stata export caption.
        /// </param>
        /// <param name="instructions">
        /// The instructions.
        /// </param>
        /// <param name="questionType">
        /// The question type.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <param name="validationExpression">
        /// The validation expression.
        /// </param>
        /// <param name="validationMessage">
        /// The validation message.
        /// </param>
        /// <param name="featured">
        /// The featured.
        /// </param>
        /// <param name="mandatory">
        /// The mandatory.
        /// </param>
        /// <param name="answerOrder">
        /// The answer order.
        /// </param>
        /// <param name="answers">
        /// The answers.
        /// </param>
        public void ChangeQuestion(
            Guid publicKey, 
            string questionText, 
            Guid targetGroupKey, 
            string stataExportCaption, 
            string instructions, 
            QuestionType questionType, 
            Guid? groupPublicKey, 
            string conditionExpression, 
            string validationExpression, 
            string validationMessage, 
            bool featured, 
            bool mandatory, 
            Order answerOrder, 
            Answer[] answers)
        {
            this.ApplyEvent(
                new QuestionChanged
                    {
                        QuestionText = questionText, 
                        TargetGroupKey = targetGroupKey, 
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
        /// The create completed q.
        /// </summary>
        /// <param name="completeQuestionnaireId">
        /// The complete questionnaire id.
        /// </param>
        public void CreateCompletedQ(Guid completeQuestionnaireId)
        {
            //// TODO: check is it good to create new AR form another?
            var cq = new CompleteQuestionnaireAR(completeQuestionnaireId, this.innerDocument);
        }

        /// <summary>
        /// The create snapshot.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.QuestionnaireDocument.
        /// </returns>
        public override QuestionnaireDocument CreateSnapshot()
        {
            return this.innerDocument;
        }

        // Event handler for the NewGroupAdded event. This method
        // is automaticly wired as event handler based on convension.

        /// <summary>
        /// The delete group.
        /// </summary>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        public void DeleteGroup(Guid groupPublicKey)
        {
            this.ApplyEvent(new GroupDeleted { GroupPublicKey = groupPublicKey });
        }

        /// <summary>
        /// The delete image.
        /// </summary>
        /// <param name="questionKey">
        /// The question key.
        /// </param>
        /// <param name="imageKey">
        /// The image key.
        /// </param>
        public void DeleteImage(Guid questionKey, Guid imageKey)
        {
            this.ApplyEvent(new ImageDeleted { ImageKey = imageKey, QuestionKey = questionKey });
        }

        /// <summary>
        /// The delete question.
        /// </summary>
        /// <param name="questionId">
        /// The question id.
        /// </param>
        public void DeleteQuestion(Guid questionId)
        {
            this.ApplyEvent(new QuestionDeleted { QuestionId = questionId });
        }

        /// <summary>
        /// The move questionnaire item.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="afterItemKey">
        /// The after item key.
        /// </param>
        public void MoveQuestionnaireItem(Guid publicKey, Guid? groupKey, Guid? afterItemKey)
        {
            this.ApplyEvent(
                new QuestionnaireItemMoved { QuestionnaireId  = this.innerDocument.PublicKey,  AfterItemKey = afterItemKey, GroupKey = groupKey, PublicKey = publicKey });
        }

        /// <summary>
        /// The restore from snapshot.
        /// </summary>
        /// <param name="snapshot">
        /// The snapshot.
        /// </param>
        public override void RestoreFromSnapshot(QuestionnaireDocument snapshot)
        {
            this.innerDocument = snapshot;
        }

        // public void UpdateGroup(string groupText, Propagate propagateble, Guid groupPublicKey, List<Guid> triggers)
        // {
        // Group group = this._innerDocument.Find<Group>(groupPublicKey);
        // if (group == null)
        // throw new ArgumentException(string.Format("group with  publick key {0} can't be found", groupPublicKey));
        // ApplyEvent(new GroupUpdated()
        // {
        // parentGroup = groupPublicKey,
        // GroupText = groupText,
        // Propagateble = propagateble,
        // Triggers = triggers
        // });
        // }

        /// <summary>
        /// The update group.
        /// </summary>
        /// <param name="groupText">
        /// The group text.
        /// </param>
        /// <param name="propagateble">
        /// The propagateble.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="executor">
        /// The executor.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void UpdateGroup(
            string groupText, 
            Propagate propagateble, 
            Guid groupPublicKey, 
            UserLight executor, 
            string conditionExpression)
        {
            var group = this.innerDocument.Find<Group>(groupPublicKey);
            if (group == null)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", groupPublicKey));
            }

            this.ApplyEvent(
                new GroupUpdated
                    {
                        GroupPublicKey = groupPublicKey, 
                        GroupText = groupText, 
                        Propagateble = propagateble, 
                        Executor = executor, 
                        ConditionExpression = conditionExpression
                    });
        }

        /// <summary>
        /// The update image.
        /// </summary>
        /// <param name="questionKey">
        /// The question key.
        /// </param>
        /// <param name="imageKey">
        /// The image key.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public void UpdateImage(Guid questionKey, Guid imageKey, string title, string description)
        {
            this.ApplyEvent(
                new ImageUpdated
                    {
                       Description = description, ImageKey = imageKey, QuestionKey = questionKey, Title = title 
                    });
        }

        /// <summary>
        /// The upload image.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="imagePublicKey">
        /// The image public key.
        /// </param>
        public void UploadImage(Guid publicKey, string title, string description, Guid imagePublicKey)
        {
            this.ApplyEvent(
                new ImageUploaded
                    {
                       Description = description, Title = title, PublicKey = publicKey, ImagePublicKey = imagePublicKey 
                    });
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on group deleted.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnGroupDeleted(GroupDeleted e)
        {
            this.innerDocument.Remove(e.GroupPublicKey);
        }

        /// <summary>
        /// The on group updated.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnGroupUpdated(GroupUpdated e)
        {
            var group = this.innerDocument.Find<Group>(e.GroupPublicKey);
            if (group != null)
            {
                group.Propagated = e.Propagateble;

                //// if(e.Triggers!=null)
                // group.Triggers = e.Triggers;
                group.ConditionExpression = e.ConditionExpression;
                group.Update(e.GroupText);
            }
        }

        /// <summary>
        /// The on image deleted.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnImageDeleted(ImageDeleted e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.QuestionKey);

            question.RemoveCard(e.ImageKey);
        }

        /// <summary>
        /// The on image updated.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnImageUpdated(ImageUpdated e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.QuestionKey);

            question.UpdateCard(e.ImageKey, e.Title, e.Description);
        }

        /// <summary>
        /// The on image uploaded.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnImageUploaded(ImageUploaded e)
        {
            var newImage = new Image
                {
                    PublicKey = e.ImagePublicKey, 
                    Title = e.Title, 
                    Description = e.Description, 
                    CreationDate = DateTime.Now
                };

            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);

            question.AddCard(newImage);
        }

        /// <summary>
        /// The on new group added.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnNewGroupAdded(NewGroupAdded e)
        {
            var group = new Group();
            group.Title = e.GroupText;
            group.Propagated = e.Paropagateble;
            group.PublicKey = e.PublicKey;
            group.ConditionExpression = e.ConditionExpression;
            this.innerDocument.Add(group, e.ParentGroupPublicKey);
        }

        /// <summary>
        /// The on new question added.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnNewQuestionAdded(NewQuestionAdded e)
        {
            AbstractQuestion result = new CompleteQuestionFactory().Create(e.QuestionType);
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
            result.Triggers.Add(e.TargetGroupKey);
            this.UpdateAnswerList(e.Answers, result);

            this.innerDocument.Add(result, e.GroupPublicKey);
        }

        /// <summary>
        /// The on new questionnaire created.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnNewQuestionnaireCreated(NewQuestionnaireCreated e)
        {
            this.innerDocument.Title = e.Title;
            this.innerDocument.PublicKey = e.PublicKey;
            this.innerDocument.CreationDate = e.CreationDate;
        }

        /// <summary>
        /// The on question changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnQuestionChanged(QuestionChanged e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);
            if (question == null)
            {
                return;
            }

            question.QuestionText = e.QuestionText;
            question.StataExportCaption = e.StataExportCaption;
            question.QuestionType = e.QuestionType;
            this.UpdateAnswerList(e.Answers, question);
            question.ConditionExpression = e.ConditionExpression;
            question.ValidationExpression = e.ValidationExpression;
            question.ValidationMessage = e.ValidationMessage;
            question.Instructions = e.Instructions;
            question.Featured = e.Featured;
            question.Mandatory = e.Mandatory;
            question.Triggers.Add(e.TargetGroupKey);
            question.AnswerOrder = e.AnswerOrder;
        }

        /// <summary>
        /// The on question deleted.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnQuestionDeleted(QuestionDeleted e)
        {
            this.innerDocument.Remove(e.QuestionId);
        }

        /// <summary>
        /// The on questionnaire item moved.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnQuestionnaireItemMoved(QuestionnaireItemMoved e)
        {
            this.innerDocument.MoveItem(e.PublicKey, e.GroupKey, e.AfterItemKey);
        }

        /// <summary>
        /// The update answer list.
        /// </summary>
        /// <param name="answers">
        /// The answers.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        protected void UpdateAnswerList(IEnumerable<Answer> answers, AbstractQuestion question)
        {
            //// List<Answer> enumerable = answers as List<Answer> ?? answers.ToList();
            if (answers != null && answers.Any())
            {
                question.Children.Clear();
                foreach (Answer answer in answers)
                {
                    question.Add(answer, question.PublicKey);
                }
            }
        }

        #endregion
    }
}