using System.Linq;

namespace Main.Core.Domain
{
    using System;
    using System.Collections.Generic;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Events.Questionnaire;
    using Main.Core.Utility;

    using Ncqrs;
    using Ncqrs.Restoring.EventStapshoot;

    public class QuestionnaireAR : SnapshootableAggregateRoot<QuestionnaireDocument>
    {
        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();
        
        private readonly ICompleteQuestionFactory questionFactory;

        public QuestionnaireAR()
        {
            this.questionFactory = new CompleteQuestionFactory();
        }

        public QuestionnaireAR(Guid publicKey, string title, Guid? createdBy = null)
            : base(publicKey)
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespaces(title);

            var clock = NcqrsEnvironment.Get<IClock>();
            this.questionFactory = new CompleteQuestionFactory();

            this.ApplyEvent(
                new NewQuestionnaireCreated
                    {
                        PublicKey = publicKey,
                        Title = title,
                        CreationDate = clock.UtcNow(),
                        CreatedBy = createdBy
                    });
        }

        public QuestionnaireAR(Guid publicKey, string title, Guid createdBy, IQuestionnaireDocument source)
            : this(publicKey, title, createdBy)
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespaces(title);

            source.Children.ApplyAction(
                x => x.Children,
                (parent, x) =>
                    {
                        var parentId = parent == null ? (Guid?)null : parent.PublicKey;

                        var q = x as IQuestion;
                        if (q != null)
                        {
                            var autoQuestion = q as IAutoPropagate;
                            this.AddQuestion(
                                publicKey: q.PublicKey,
                                questionText: q.QuestionText,
                                stataExportCaption: q.StataExportCaption,
                                questionType: q.QuestionType,
                                questionScope: q.QuestionScope,
                                conditionExpression: q.ConditionExpression,
                                validationExpression: q.ValidationExpression,
                                validationMessage: q.ValidationMessage,
                                featured: q.Featured,
                                mandatory: q.Mandatory,
                                capital: q.Capital,
                                answerOrder: q.AnswerOrder,
                                instructions: q.Instructions,
                                groupPublicKey: parentId,
                                triggers: autoQuestion == null ? null : autoQuestion.Triggers,
                                maxValue: autoQuestion == null ? 0 : autoQuestion.MaxValue,
                                answers: q.Answers.Select(Answer.CreateFromOther).ToArray());
                        }
                        var g = x as IGroup;
                        if (g != null)
                        {
                            this.AddGroup(
                                publicKey: g.PublicKey,
                                text: g.Title,
                                propagateble: g.Propagated,
                                parentGroupKey: parentId,
                                conditionExpression: g.ConditionExpression,
                                description: g.Description);
                        }
                    });
        }

        public override QuestionnaireDocument CreateSnapshot()
        {
            return this.innerDocument;
        }

        public override void RestoreFromSnapshot(QuestionnaireDocument snapshot)
        {
            this.innerDocument = snapshot.Clone() as QuestionnaireDocument;
        }

        public void UpdateQuestionnaire(string title)
        #warning CRUD
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespaces(title);

            this.ApplyEvent(new QuestionnaireUpdated() { Title = title });
        }

        public void DeleteQuestionnaire()
        {
            this.ApplyEvent(new QuestionnaireDeleted());
        }

        [Obsolete]
        public void AddGroup(
            Guid publicKey, string text, Propagate propagateble, Guid? parentGroupKey, string conditionExpression, string description)
        {
            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespaces(text);
            
            this.ApplyEvent(
                new NewGroupAdded
                    {
                        PublicKey = publicKey,
                        GroupText = text,
                        ParentGroupPublicKey = parentGroupKey,
                        Paropagateble = propagateble,
                        ConditionExpression = conditionExpression,
                        Description = description
                    });
        }

        [Obsolete]
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

            this.ThrowDomainExceptionIfAnswersNeededButAbsent(questionType, answers);

            this.ThrowDomainExceptionIfAnswerValuesContainsInvalidCharacters(questionType, answers);

            this.ThrowDomainExceptionIfStataCaptionIsInvalid(publicKey, stataExportCaption);

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

        [Obsolete]
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
            this.ThrowDomainExceptionIfQuestionDoesNotExist(publicKey);

            stataExportCaption = stataExportCaption.Trim();

            this.ThrowDomainExceptionIfStataCaptionIsInvalid(publicKey, stataExportCaption);

            this.ThrowDomainExceptionIfAnswersNeededButAbsent(questionType, answers);

            this.ThrowDomainExceptionIfAnswerValuesContainsInvalidCharacters(questionType, answers);

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

        public void CreateCompletedQ(Guid completeQuestionnaireId, UserLight creator)
        #warning probably a factory should be used here
        {
            // TODO: check is it good to create new AR form another?
            // Do we need Saga here?
            new CompleteQuestionnaireAR(completeQuestionnaireId, this.innerDocument, creator);
        }

        [Obsolete]
        public void DeleteGroup(Guid groupPublicKey, Guid parentPublicKey)
        #warning we should not supply parent here. that is because question is unique, and parent has no business sense
        {
            this.ApplyEvent(new GroupDeleted(groupPublicKey, parentPublicKey));
        }

        public void DeleteImage(Guid questionKey, Guid imageKey)
        {
            this.ApplyEvent(new ImageDeleted { ImageKey = imageKey, QuestionKey = questionKey });
        }

        [Obsolete]
        public void DeleteQuestion(Guid questionId, Guid parentPublicKey)
        #warning we should not supply parent here. that is because question is unique, and parent has no business sense
        {
            this.ApplyEvent(new QuestionDeleted(questionId, parentPublicKey));
        }

        public void MoveQuestionnaireItem(Guid publicKey, Guid? groupKey, Guid? afterItemKey)
        {
            this.ApplyEvent(
                new QuestionnaireItemMoved
                    {
                        AfterItemKey = afterItemKey,
                        GroupKey = groupKey,
                        PublicKey = publicKey
                    });
        }

        public void NewAddGroup(Guid groupId,
            Guid? parentGroupId, string title, Propagate propagationKind, string description, string condition)
        {
            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespaces(title);

            this.ThrowDomainExceptionIfGroupsPropagationKindIsNotSupported(propagationKind);

            this.ApplyEvent(new NewGroupAdded
            {
                PublicKey = groupId,
                GroupText = title,
                ParentGroupPublicKey = parentGroupId,
                Paropagateble = propagationKind,
                Description = description,
                ConditionExpression = condition,
            });
        }

        public void NewDeleteGroup(Guid groupId)
        {
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);

            this.ApplyEvent(new GroupDeleted(groupId));
        }

        public void NewUpdateGroup(Guid groupId,
            string title, Propagate propagationKind, string description, string condition)
        {
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);

            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespaces(title);

            this.ThrowDomainExceptionIfGroupsPropagationKindIsNotSupported(propagationKind);

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

        public void NewAddQuestion(Guid questionId,
            Guid groupId, string title, QuestionType type, string alias, 
            bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup, 
            QuestionScope scope, string condition, string validationExpression, string validationMessage,
            string instructions, Option[] options, Order optionsOrder, int? maxValue, Guid[] triggedGroupIds)
        {
            alias = alias.Trim();

            this.ThrowDomainExceptionIfOptionsNeededButAbsent(type, options);
            this.ThrowDomainExceptionIfTitleisEmpty(title);
            this.ThrowDomainExceptionIfOptionsContainsInvalidCharacters(type, options);

            this.ThrowDomainExceptionIfStataCaptionIsInvalid(questionId, alias);

            this.ThrowDomainExceptionIfQuestionIsFeaturedButNotInsideNonPropagateGroup(questionId, isFeatured, groupId);

            this.ThrowDomainExceptionIfQuestionIsHeadOfGroupButNotInsidePropagateGroup(questionId, isHeaderOfPropagatableGroup, groupId);

            this.ApplyEvent(new NewQuestionAdded
            {
                PublicKey = questionId,

                GroupPublicKey = groupId,
                QuestionText = title,
                QuestionType = type,
                StataExportCaption = alias,

                Mandatory = isMandatory,
                Featured = isFeatured,
                Capital = isHeaderOfPropagatableGroup,

                QuestionScope = scope,
                ConditionExpression = condition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,

                Answers = ConvertOptionsToAnswers(options),
                AnswerOrder = optionsOrder,
                MaxValue = maxValue ?? 10,
                Triggers = triggedGroupIds != null ? triggedGroupIds.ToList() : null,
            });
        }

        public void NewDeleteQuestion(Guid questionId)
        {
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);

            this.ApplyEvent(new QuestionDeleted(questionId));
        }

        public void NewUpdateQuestion(Guid questionId,
            string title, QuestionType type, string alias,
            bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage,
            string instructions, Option[] options, Order optionsOrder, int? maxValue, Guid[] triggedGroupIds)
        {
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
          
            alias = alias.Trim();

            this.ThrowDomainExceptionIfStataCaptionIsInvalid(questionId, alias);
            this.ThrowDomainExceptionIfTitleisEmpty(title);
            this.ThrowDomainExceptionIfOptionsNeededButAbsent(type, options);
            

            this.ThrowDomainExceptionIfQuestionIsFeaturedButNotInsideNonPropagateGroup(questionId, isFeatured, null);

            this.ThrowDomainExceptionIfQuestionIsHeadOfGroupButNotInsidePropagateGroup(questionId, isHeaderOfPropagatableGroup, null);


            this.ApplyEvent(new QuestionChanged
            {
                PublicKey = questionId,

                QuestionText = title,
                QuestionType = type,
                StataExportCaption = alias,

                Mandatory = isMandatory,
                Featured = isFeatured,
                Capital = isHeaderOfPropagatableGroup,

                QuestionScope = scope,
                ConditionExpression = condition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,

                Answers = ConvertOptionsToAnswers(options),
                AnswerOrder = optionsOrder,
                MaxValue = maxValue ?? 10,
                Triggers = triggedGroupIds != null ? triggedGroupIds.ToList() : null,
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
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupPublicKey);

            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespaces(groupText);

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

        protected void OnQuestionnaireUpdated(QuestionnaireUpdated e)
        {
            this.innerDocument.Title = e.Title;
        }

        protected void OnQuestionnaireDeleted(QuestionnaireDeleted e)
        {
            this.innerDocument.IsDeleted = true;
        }

        protected void OnGroupDeleted(GroupDeleted e)
        {
            this.innerDocument.RemoveGroup(e.GroupPublicKey);
        }

        protected void OnGroupUpdated(GroupUpdated e)
        {
            var group = this.innerDocument.Find<Group>(e.GroupPublicKey);
            if (group != null)
            {
                if (group.Propagated != Propagate.None)
                {
                    this.innerDocument.UpdateAutoPropagateQuestionsTriggersIfNeeded(group);
                }
                group.Propagated = e.Propagateble;

                group.ConditionExpression = e.ConditionExpression;
                group.Description = e.Description;
                group.Update(e.GroupText);
            }
        }

        protected void OnImageDeleted(ImageDeleted e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.QuestionKey);

            question.RemoveCard(e.ImageKey);
        }

        protected void OnImageUpdated(ImageUpdated e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.QuestionKey);
            if (question == null)
            {
                return;
            }

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

            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);

            question.AddCard(newImage);
        }

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

        protected void OnNewQuestionAdded(NewQuestionAdded e)
        {
            AbstractQuestion question = new CompleteQuestionFactory().Create(e);
            if (question == null)
            {
                return;
            }

            this.innerDocument.Add(question, e.GroupPublicKey, null);
        }

        protected void OnNewQuestionnaireCreated(NewQuestionnaireCreated e)
        {
            this.innerDocument.Title = e.Title;
            this.innerDocument.PublicKey = e.PublicKey;
            this.innerDocument.CreationDate = e.CreationDate;
            this.innerDocument.LastEntryDate = e.CreationDate;
            this.innerDocument.CreatedBy = e.CreatedBy;
        }

        protected void OnQuestionChanged(QuestionChanged e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);
            IQuestion newQuestion = this.questionFactory.CreateQuestionFromExistingUsingDataFromEvent(question, e);
            this.innerDocument.ReplaceQuestionWithNew(question, newQuestion);
        }

        protected void OnQuestionDeleted(QuestionDeleted e)
        {
            this.innerDocument.RemoveQuestion(e.QuestionId);
        }

        protected void OnQuestionnaireItemMoved(QuestionnaireItemMoved e)
        {
            this.innerDocument.MoveItem(e.PublicKey, e.GroupKey, e.AfterItemKey);
        }

        private static Answer[] ConvertOptionsToAnswers(Option[] options)
        {
            if (options == null)
                return null;

            return options.Select(ConvertOptionToAnswer).ToArray();
        }

        private static Answer ConvertOptionToAnswer(Option option)
        {
            return new Answer
            {
                PublicKey = option.Id,
                AnswerType = AnswerType.Select,
                AnswerValue = option.Value,
                AnswerText = option.Title,
            };
        }

        private void ThrowDomainExceptionIfQuestionIsHeadOfGroupButNotInsidePropagateGroup(Guid questionId, bool isHeadOfGroup, Guid? groupId)
        {
            if (!isHeadOfGroup)
                return;

            IGroup group;
            if (groupId.HasValue)
            {
                group = this.innerDocument.Find<Group>(groupId.Value);
            }
            else
            {
                this.innerDocument.ConnectChildsWithParent();
                var question = this.innerDocument.Find<AbstractQuestion>(questionId);
                group = question.GetParent() as IGroup;
            }

            if (group.Propagated == Propagate.None)
            {
                throw new DomainException("Question inside propagated group can not be featured");
            }
        }

        private void ThrowDomainExceptionIfQuestionIsFeaturedButNotInsideNonPropagateGroup(Guid questionId, bool isFeatured, Guid? groupId)
        {
            if (!isFeatured)
                return;
            
            IGroup group;
            if (groupId.HasValue)
            {
                group = this.innerDocument.Find<Group>(groupId.Value);
            }
            else
            {
                this.innerDocument.ConnectChildsWithParent();
                var question = this.innerDocument.Find<AbstractQuestion>(questionId);
                group = question.GetParent() as IGroup;
            }

            if (group.Propagated != Propagate.None)
            {
                throw new DomainException("Question inside propagated group can not be featured");
            }
        }

        private void ThrowDomainExceptionIfGroupsPropagationKindIsNotSupported(Propagate propagationKind)
        {
            if (!(propagationKind == Propagate.None || propagationKind == Propagate.AutoPropagated))
                throw new DomainException(string.Format("Group's propagation kind {0} is unsupported", propagationKind));
        }

        private void ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespaces(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new DomainException("The titles of groups and chapters can not be empty or contains whitespaces only");
            }
        }

        private void ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespaces(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new DomainException("Questionnaire's title can not be empty or contains whitespaces only");
            }
        }

        private void ThrowDomainExceptionIfQuestionDoesNotExist(Guid publicKey)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(publicKey);
            if (question == null)
            {
                throw new DomainException(string.Format("Question with public key {0} can't be found", publicKey));
            }
        }

        private void ThrowDomainExceptionIfGroupDoesNotExist(Guid groupPublicKey)
        {
            var group = this.innerDocument.Find<Group>(groupPublicKey);
            if (group == null)
            {
                throw new DomainException(string.Format("group with  publick key {0} can't be found", groupPublicKey));
            }
        }
        private void ThrowDomainExceptionIfTitleisEmpty(string title)
        {
           if(string.IsNullOrEmpty(title))
               throw new DomainException("Question title can't be empty");
        }
        private void ThrowDomainExceptionIfOptionsNeededButAbsent(QuestionType type, Option[] options)
        {
            this.ThrowDomainExceptionIfAnswersNeededButAbsent(type, ConvertOptionsToAnswers(options));
        }

        private void ThrowDomainExceptionIfAnswersNeededButAbsent(QuestionType questionType,
                                                                  IEnumerable<IAnswer> answerOptions)
        {
            var isQuestionWithOptions = questionType == QuestionType.MultyOption ||
                                        questionType == QuestionType.SingleOption;
            if (!isQuestionWithOptions)
                return;
            if (!answerOptions.Any())
                throw new DomainException("Questions with options should have one answer option at least");
            Dictionary<string, int> uniquniesTitleCounter = new Dictionary<string, int>();
            foreach (var answerOption in answerOptions)
            {
                if(string.IsNullOrEmpty(answerOption.AnswerText))
                    throw new DomainException("Answer title can't be empty");
                if (uniquniesTitleCounter.ContainsKey(answerOption.AnswerText))
                    throw new DomainException("Answer title is not unique");
                uniquniesTitleCounter[answerOption.AnswerText] = 1;
            }

        }

        private void ThrowDomainExceptionIfStataCaptionIsInvalid(Guid questionPublicKey, string stataCaption)
        {
            if (string.IsNullOrEmpty(stataCaption))
            {
                throw new DomainException("Variable name shouldn't be empty or contains white spaces");
            }

            bool isTooLong = stataCaption.Length > 32;
            if (isTooLong)
            {
                throw new DomainException("Variable name shouldn't be longer than 32 characters");
            }

            bool containsInvalidCharacters = stataCaption.Any(c => !(c == '_' || Char.IsLetterOrDigit(c)));
            if (containsInvalidCharacters)
            {
                throw new DomainException("Valid variable name should contains only letters, digits and underscore character");
            }

            bool startsWithDigit = Char.IsDigit(stataCaption[0]);
            if (startsWithDigit)
            {
                throw new DomainException("Variable name shouldn't starts with digit");
            }

            var captions = this.innerDocument.GetAllQuestions<AbstractQuestion>()
                               .Where(q => q.PublicKey != questionPublicKey)
                               .Select(q => q.StataExportCaption);

            bool isNotUnique = captions.Contains(stataCaption);
            if (isNotUnique)
            {
                throw new DomainException("Variable name should be unique in questionnaire's scope");
            }
        }

        private void ThrowDomainExceptionIfOptionsContainsInvalidCharacters(QuestionType type, Option[] options)
        {
            this.ThrowDomainExceptionIfAnswerValuesContainsInvalidCharacters(type, ConvertOptionsToAnswers(options));
        }

        private void ThrowDomainExceptionIfAnswerValuesContainsInvalidCharacters(
            QuestionType questionType, IEnumerable<IAnswer> answerOptions)
        {
            var isQuestionWithOptions = questionType == QuestionType.MultyOption
                                        || questionType == QuestionType.SingleOption;
            int iAnswerValue = 0;
            if (isQuestionWithOptions && answerOptions.Any(x=>!int.TryParse(x.AnswerValue, out iAnswerValue)))
            {
                throw new DomainException("Answer values should have only number characters");
            }
        }
    }
}