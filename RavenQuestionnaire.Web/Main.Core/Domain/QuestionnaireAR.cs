// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireAR.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Questionnaire Aggregate Root.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Linq;

namespace Main.Core.Domain
{
    using System;
    using System.Collections.Generic;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Questionnaire;

    using Ncqrs;
    using Ncqrs.Restoring.EventStapshoot;

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
        private readonly ICompleteQuestionFactory questionFactory;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireAR"/> class.
        /// </summary>
        public QuestionnaireAR()
        {
            this.questionFactory = new CompleteQuestionFactory();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireAR"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The questionnaire id.
        /// </param>
        /// <param name="title">
        /// The text.
        /// </param>
        /// <param name="createdBy">
        /// The created by.
        /// </param>
        public QuestionnaireAR(Guid publicKey, string title, Guid? createdBy = null)
            : base(publicKey)
        {
            var clock = NcqrsEnvironment.Get<IClock>();
            this.questionFactory = new CompleteQuestionFactory();

            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewQuestionnaireCreated method).
            this.ApplyEvent(
                new NewQuestionnaireCreated
                    {
                        PublicKey = publicKey,
                        Title = title,
                        CreationDate = clock.UtcNow(),
                        CreatedBy = createdBy
                    });
        }

        #endregion

        #region Public Methods and Operators

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

        /// <summary>
        /// The restore from snapshot.
        /// </summary>
        /// <param name="snapshot">
        /// The snapshot.
        /// </param>
        public override void RestoreFromSnapshot(QuestionnaireDocument snapshot)
        {
            this.innerDocument = snapshot.Clone() as QuestionnaireDocument;
        }

        /// <summary>
        /// The update questionnaire.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        public void UpdateQuestionnaire(string title)
        #warning CRUD
        {
            this.ApplyEvent(new QuestionnaireUpdated() { Title = title });
        }

        /// <summary>
        /// The delete questionnaire.
        /// </summary>
        public void DeleteQuestionnaire()
        {
            this.ApplyEvent(new QuestionnaireDeleted());
        }

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
        /// The propagate.
        /// </param>
        /// <param name="parentGroupKey">
        /// The parent group key.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        [Obsolete]
        public void AddGroup(
            Guid publicKey, string text, Propagate propagateble, Guid? parentGroupKey, string conditionExpression, string description)
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
                        ConditionExpression = conditionExpression,
                        Description = description
                    });
        }

        /// <summary>
        /// The add question.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionText">
        /// The question text.
        /// </param>
        /// <param name="stataExportCaption">
        /// The stata export caption.
        /// </param>
        /// <param name="questionType">
        /// The question type.
        /// </param>
        /// <param name="questionScope">
        /// The question scope
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
        /// <param name="capital">
        /// The capital
        /// </param>
        /// <param name="answerOrder">
        /// The answer order.
        /// </param>
        /// <param name="instructions">
        /// The instructions.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="triggers">
        /// The triggers
        /// </param>
        /// <param name="maxValue">
        /// The max value of autopropagate question
        /// </param>
        /// <param name="answers">
        /// The answers.
        /// </param>
        public void AddQuestion(
            Guid publicKey,
            string questionText,
            string stataExportCaption,
            QuestionType questionType,
            QuestionScope questionScope,
            string conditionExpression,
            string validationExpression,
            string validationMessage,
            bool featured,
            bool mandatory,
            bool capital,
            Order answerOrder,
            string instructions,
            Guid? groupPublicKey,
            List<Guid> triggers,
            int maxValue,
            Answer[] answers)
        {
            stataExportCaption = stataExportCaption.Trim();

            this.ThrowArgumentExceptionIfAnswersNeededButAbsent(questionType, answers);

            ThrowArgumentExceptionIfStataCaptionIsInvalid(publicKey, stataExportCaption);

            this.ApplyEvent(
                new NewQuestionAdded
                    {
                        PublicKey = publicKey,
                        QuestionText = questionText,
                        StataExportCaption = stataExportCaption,
                        QuestionType = questionType,
                        QuestionScope = questionScope,
                        ConditionExpression = conditionExpression,
                        ValidationExpression = validationExpression,
                        ValidationMessage = validationMessage,
                        Featured = featured,
                        Mandatory = mandatory,
                        Capital = capital,
                        AnswerOrder = answerOrder,
                        GroupPublicKey = groupPublicKey,
                        Triggers = triggers,
                        MaxValue = maxValue,
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
        /// <param name="triggers">
        /// List of triggers for autopropagate question
        /// </param>
        /// <param name="maxValue">
        /// The max value of autopropagate question
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
        /// <param name="questionScope">
        /// The question scope
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
        /// <param name="capital">
        /// The capital
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
            List<Guid> triggers,
            int maxValue,
            string stataExportCaption,
            string instructions,
            QuestionType questionType,
            QuestionScope questionScope,
            Guid? groupPublicKey,
            string conditionExpression,
            string validationExpression,
            string validationMessage,
            bool featured,
            bool mandatory,
            bool capital,
            Order answerOrder,
            Answer[] answers)
        {
            ThrowArgumentExceptionIfQuestionDoesntExist(publicKey);

            stataExportCaption = stataExportCaption.Trim();

            ThrowArgumentExceptionIfStataCaptionIsInvalid(publicKey, stataExportCaption);

            this.ThrowArgumentExceptionIfAnswersNeededButAbsent(questionType, answers);

            this.ApplyEvent(
                new QuestionChanged
                    {
                        PublicKey = publicKey,
                        QuestionText = questionText,
                        Triggers = triggers,
                        MaxValue = maxValue,
                        StataExportCaption = stataExportCaption,
                        QuestionType = questionType,
                        QuestionScope = questionScope,
                        ConditionExpression = conditionExpression,
                        ValidationExpression = validationExpression,
                        ValidationMessage = validationMessage,
                        Featured = featured,
                        Mandatory = mandatory,
                        Capital = capital,
                        AnswerOrder = answerOrder,
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
        /// <param name="creator">
        /// The creator.
        /// </param>
        public void CreateCompletedQ(Guid completeQuestionnaireId, UserLight creator)
        #warning probably a factory should be used here
        {
            // TODO: check is it good to create new AR form another?
            // Do we need Saga here?
            var cq = new CompleteQuestionnaireAR(completeQuestionnaireId, this.innerDocument, creator);
        }

        /// <summary>
        /// The delete group.
        /// </summary>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="parentPublicKey">
        /// The parent Public Key.
        /// </param>
        [Obsolete]
        public void DeleteGroup(Guid groupPublicKey, Guid parentPublicKey)
        #warning we should not supply parent here. that is because question is unique, and parent has no business sense
        {
            this.ApplyEvent(new GroupDeleted(groupPublicKey, parentPublicKey));
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
        /// <param name="parentPublicKey">
        /// The parent Public Key.
        /// </param>
        public void DeleteQuestion(Guid questionId, Guid parentPublicKey)
#warning we should not supply parent here. that is because question is unique, and parent has no business sense
        {
            this.ApplyEvent(new QuestionDeleted(questionId, parentPublicKey));
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
                new QuestionnaireItemMoved
                    {
                        QuestionnaireId = this.innerDocument.PublicKey,
                        AfterItemKey = afterItemKey,
                        GroupKey = groupKey,
                        PublicKey = publicKey
                    });
        }

        public void NewAddGroup(){}

        public void NewDeleteGroup(Guid groupId)
        {
            this.ThrowArgumentExceptionIfGroupDoesNotExist(groupId);

            this.ApplyEvent(new GroupDeleted(groupId));
        }

        public void NewUpdateGroup(Guid groupId,
            string title, Propagate propagationKind, string description, string condition)
        {
            this.ThrowArgumentExceptionIfGroupDoesNotExist(groupId);

            this.ApplyEvent(new GroupUpdated
            {
                QuestionnaireId = this.innerDocument.PublicKey.ToString(),
                GroupPublicKey = groupId,
                GroupText = title,
                Propagateble = propagationKind,
                Description = description,
                ConditionExpression = condition,
            });
        }

        [Obsolete]
        public void UpdateGroup(
            string groupText,
            Propagate propagateble,
            Guid groupPublicKey,
            UserLight executor,
            string conditionExpression,
            string description)
#warning get rid of executor here and create a common mechanism for handling it if needed
        {
            this.ThrowArgumentExceptionIfGroupDoesNotExist(groupPublicKey);

            this.ApplyEvent(
                new GroupUpdated
                    {
                        QuestionnaireId = this.innerDocument.PublicKey.ToString(),
                        GroupPublicKey = groupPublicKey,
                        GroupText = groupText,
                        Propagateble = propagateble,
                        /*Executor = executor,*/
                        ConditionExpression = conditionExpression,
                        Description = description
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
                        Description = description,
                        ImageKey = imageKey,
                        QuestionKey = questionKey,
                        Title = title
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
                        Description = description,
                        Title = title,
                        PublicKey = publicKey,
                        ImagePublicKey = imagePublicKey
                    });
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on questionnaire updated.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnQuestionnaireUpdated(QuestionnaireUpdated e)
        {
            this.innerDocument.Title = e.Title;
        }

        /// <summary>
        /// The on questionnaire deleted.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnQuestionnaireDeleted(QuestionnaireDeleted e)
        {
            this.innerDocument.IsDeleted = true;
        }

        /// <summary>
        /// The on group deleted.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnGroupDeleted(GroupDeleted e)
        {
            this.innerDocument.RemoveGroup(e.GroupPublicKey);
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
                group.Description = e.Description;
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
            if (question == null)
            {
                return;
            }

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
            group.Description = e.Description;
            group.ConditionExpression = e.ConditionExpression;
            this.innerDocument.Add(group, e.ParentGroupPublicKey, null);
        }

        /// <summary>
        /// The on new question added.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnNewQuestionAdded(NewQuestionAdded e)
        {
            AbstractQuestion question = new CompleteQuestionFactory().Create(e);
            if (question == null)
            {
                return;
            }

            this.innerDocument.Add(question, e.GroupPublicKey, null);
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
            this.innerDocument.LastEntryDate = e.CreationDate;
            this.innerDocument.CreatedBy = e.CreatedBy;
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
            this.questionFactory.UpdateQuestionByEvent(question, e);
        }

        /// <summary>
        /// The on question deleted.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnQuestionDeleted(QuestionDeleted e)
        {
            this.innerDocument.Remove(e.QuestionId, null, e.ParentPublicKey, null);
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

        #endregion

        private void ThrowArgumentExceptionIfQuestionDoesntExist(Guid publicKey)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(publicKey);
            if (question == null)
            {
                throw new ArgumentException(string.Format("Question with public key {0} can't be found", publicKey));
            }
        }

        private void ThrowArgumentExceptionIfGroupDoesNotExist(Guid groupPublicKey)
        {
            var group = this.innerDocument.Find<Group>(groupPublicKey);
            if (group == null)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", groupPublicKey));
            }
        }

        private void ThrowArgumentExceptionIfAnswersNeededButAbsent(QuestionType questionType, Answer[] answerOptions)
        {
            var isQuestionWithOptions = questionType == QuestionType.MultyOption || questionType == QuestionType.SingleOption;
            if (isQuestionWithOptions && answerOptions.Length == 0)
            {
                throw new ArgumentException("Questions with options should have one answer option at least", "QuestionType");
            }
        }

        private void ThrowArgumentExceptionIfStataCaptionIsInvalid(Guid questionPublicKey, string stataCaption)
        {
            if (string.IsNullOrEmpty(stataCaption))
            {
                throw new ArgumentException("Variable name shouldn't be empty or contains white spaces", "StataExportCaption");
            }

            bool isTooLong = stataCaption.Length > 32;
            if (isTooLong)
            {
                throw new ArgumentException("Variable name shouldn't be longer than 32 characters", "StataExportCaption");
            }

            bool containsInvalidCharacters = stataCaption.Any(c => !(c == '_' || Char.IsLetterOrDigit(c)));
            if (containsInvalidCharacters)
            {
                throw new ArgumentException("Valid variable name should contains only letters, digits and underscore character", "StataExportCaption");
            }

            bool startsWithDigit = Char.IsDigit(stataCaption[0]);
            if (startsWithDigit)
            {
                throw new ArgumentException("Variable name shouldn't starts with digit", "StataExportCaption");
            }

            var captions = this.innerDocument.GetAllQuestions<AbstractQuestion>()
                               .Where(q => q.PublicKey != questionPublicKey)
                               .Select(q => q.StataExportCaption);

            bool isNotUnique = captions.Contains(stataCaption);
            if (isNotUnique)
            {
                throw new ArgumentException("Variable name should be unique in questionnaire's scope", "StataExportCaption");
            }
        }
    }
}