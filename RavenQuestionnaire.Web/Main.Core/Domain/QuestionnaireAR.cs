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
#warning 'if MONODROID' is bad. should use abstract logger (ILogger?) which implementation will be different in different apps
#if MONODROID
        private static readonly AndroidLogger.ILog Logger = AndroidLogger.LogManager.GetLogger(typeof(QuestionnaireAR));
#else
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
#endif

        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();
        
        private readonly ICompleteQuestionFactory questionFactory;

        private static readonly HashSet<QuestionType> AllowedQuestionTypes = new HashSet<QuestionType>
        {
            QuestionType.SingleOption,
            QuestionType.MultyOption,
            QuestionType.Numeric,
            QuestionType.DateTime,
            QuestionType.Text,
            QuestionType.AutoPropagate,
        };

        public QuestionnaireAR()
            : base()
        {
            this.questionFactory = new CompleteQuestionFactory();
        }

        public QuestionnaireAR(Guid publicKey)
            : base(publicKey)
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
            source.Children.ApplyAction(
                x => x.Children,
                (parent, x) =>
                    {
                        Guid? parentId = parent == null ? (Guid?)null : parent.PublicKey;

                        var q = x as IQuestion;
                        if (q != null)
                        {
                            var autoQuestion = q as IAutoPropagate;
                            this.NewAddQuestion(
                                questionId: q.PublicKey,
                                groupId: parentId.Value,
                                title: q.QuestionText,
                                type: q.QuestionType,
                                alias: q.StataExportCaption,
                                isMandatory: q.Mandatory,
                                isFeatured: q.Featured,
                                isHeaderOfPropagatableGroup: q.Capital,
                                scope: q.QuestionScope,
                                condition: q.ConditionExpression,
                                validationExpression: q.ValidationExpression,
                                validationMessage: q.ValidationMessage,
                                instructions: q.Instructions,
                                options: q.Answers.Select(ConvertAnswerToOption).ToArray(),
                                optionsOrder: q.AnswerOrder,
                                maxValue: autoQuestion == null ? 0 : autoQuestion.MaxValue,
                                triggedGroupIds: autoQuestion == null ? null : autoQuestion.Triggers.ToArray());
                        }
                        var g = x as IGroup;
                        if (g != null)
                        {
                            this.NewAddGroup(
                                groupId: g.PublicKey,
                                parentGroupId: parentId,
                                title: g.Title,
                                propagationKind: g.Propagated,
                                description: g.Description,
                                condition: g.ConditionExpression);
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

        public void CreateCompletedQ(Guid completeQuestionnaireId, UserLight creator)
        #warning probably a factory should be used here
        {
            // TODO: check is it good to create new AR form another?
            // Do we need Saga here?
            new CompleteQuestionnaireAR(completeQuestionnaireId, this.innerDocument, creator);
        }

        public void DeleteImage(Guid questionKey, Guid imageKey)
        {
            this.ApplyEvent(new ImageDeleted { ImageKey = imageKey, QuestionKey = questionKey });
        }

        [Obsolete]
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
            this.ThrowDomainExceptionIfParentGroupCantHaveChildGroups(parentGroupId);

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

        public void MoveGroup(Guid groupId, Guid? targetGroupId, int targetIndex)
        {
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);

            if (targetGroupId.HasValue)
            {
                this.ThrowDomainExceptionIfGroupDoesNotExist(targetGroupId.Value);
            }

            this.ApplyEvent(new QuestionnaireItemMoved
            {
                PublicKey = groupId,
                GroupKey = targetGroupId,
                TargetIndex = targetIndex,
            });
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

            this.ThrowDomainExceptionIfTitleIsEmpty(title);
            
            this.ThrowDomainExceptionIfStataCaptionIsInvalid(questionId, alias);

            this.ThrowDomainExceptionIfQuestionWithOptionsIsInvalid(type, options);

            var group = this.innerDocument.Find<IGroup>(groupId);
            this.ThrowDomainExceptionIfQuestionIsFeaturedButGroupIsPropagated(isFeatured, group);
            this.ThrowDomainExceptionIfQuestionIsHeadOfGroupButGroupIsNotPropagated(isHeaderOfPropagatableGroup, group);

            this.ThrowDomainExceptionIfAnyTriggerLinksToAbsentOrNotPropagatedGroup(type, triggedGroupIds);

            this.ThrowDomainExceptionIfQuestionTypeIsNotAllowed(type);

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

        public void MoveQuestion(Guid questionId, Guid targetGroupId, int targetIndex)
        {
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(targetGroupId);

            this.ApplyEvent(new QuestionnaireItemMoved
            {
                PublicKey = questionId,
                GroupKey = targetGroupId,
                TargetIndex = targetIndex,
            });
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
            this.ThrowDomainExceptionIfTitleIsEmpty(title);
            this.ThrowDomainExceptionIfQuestionWithOptionsIsInvalid(type, options);

            this.ThrowDomainExceptionIfQuestionTypeIsNotAllowed(type);

            IGroup group = this.innerDocument.GetParentOfQuestion(questionId);
            this.ThrowDomainExceptionIfQuestionIsFeaturedButGroupIsPropagated(isFeatured, group);
            this.ThrowDomainExceptionIfQuestionIsHeadOfGroupButGroupIsNotPropagated(isHeaderOfPropagatableGroup, group);

            this.ThrowDomainExceptionIfAnyTriggerLinksToAbsentOrNotPropagatedGroup(type, triggedGroupIds);

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
            this.innerDocument.UpdateGroup(e.GroupPublicKey, e.GroupText, e.Description, e.Propagateble, e.ConditionExpression);
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
            bool isLegacyEvent = e.AfterItemKey != null;

            if (isLegacyEvent)
            {
                Logger.Warn(string.Format("Ignored legacy MoveItem event in questionnaire {0}", this.EventSourceId));
                return;
            }

            this.innerDocument.MoveItem(e.PublicKey, e.GroupKey, e.TargetIndex);
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

        private static Option ConvertAnswerToOption(IAnswer answer)
        {
            return new Option(answer.PublicKey, answer.AnswerValue, answer.AnswerText);
        }

        private void ThrowDomainExceptionIfParentGroupCantHaveChildGroups(Guid? parentGroupId)
        {
            bool isParentGroupAChapter = !parentGroupId.HasValue;
            if (isParentGroupAChapter)
                return;

            var parentGroup = this.innerDocument.Find<Group>(parentGroupId.Value);
            if (parentGroup.Propagated == Propagate.AutoPropagated)
            {
                throw new DomainException(DomainExceptionType.AutoPropagateGroupCantHaveChildGroups, "Auto propagated groups can't have child groups");
            }
        }

        private void ThrowDomainExceptionIfAnyTriggerLinksToAbsentOrNotPropagatedGroup(QuestionType type, Guid[] triggedGroupIds)
        {
            bool questionIsNotAutoPropagated = type != QuestionType.AutoPropagate;
            bool noGroupsShouldBeTrigged = triggedGroupIds == null || triggedGroupIds.Length == 0;
            if (questionIsNotAutoPropagated || noGroupsShouldBeTrigged)
                return;

            foreach (Guid groupId in triggedGroupIds)
            {
                var group = this.innerDocument.Find<Group>(groupId);
                if (@group == null)
                {
                    throw new DomainException(
                        DomainExceptionType.TriggerLinksToNotExistingGroup, "Question can trigger only existing group");
                }

                if (@group.Propagated != Propagate.AutoPropagated)
                {
                    throw new DomainException(DomainExceptionType.TriggerLinksToNotPropagatedGroup, 
                        string.Format("Group {0} cannot be triggered because it is not auto propagated", group.Title));
                }
            }
        }

        private void ThrowDomainExceptionIfQuestionIsHeadOfGroupButGroupIsNotPropagated(bool isHeadOfGroup, IGroup group)
        {
            if (!isHeadOfGroup)
                return;

            if (group.Propagated == Propagate.None)
            {
                throw new DomainException(
                     DomainExceptionType.QuestionIsHeadOfGroupButNotInsidePropagateGroup,
                     "Question inside propagated group can not be head of group");
            }
        }

        private void ThrowDomainExceptionIfQuestionIsFeaturedButGroupIsPropagated(bool isFeatured, IGroup group)
        {
            if (!isFeatured)
                return;

            if (group.Propagated != Propagate.None)
            {
                throw new DomainException(
                    DomainExceptionType.QuestionIsFeaturedButNotInsideNonPropagateGroup,
                    "Question inside propagated group can not be featured");
            }
        }

        private void ThrowDomainExceptionIfGroupsPropagationKindIsNotSupported(Propagate propagationKind)
        {
            if (!(propagationKind == Propagate.None || propagationKind == Propagate.AutoPropagated))
                throw new DomainException(
                    DomainExceptionType.NotSupportedPropagationGroup,
                    string.Format("Group's propagation kind {0} is unsupported", propagationKind));
        }

        private void ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespaces(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new DomainException(
                    DomainExceptionType.GroupTitleRequired,
                    "The titles of groups and chapters can not be empty or contains whitespaces only");
            }
        }

        private void ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespaces(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new DomainException(
                    DomainExceptionType.QuestionnaireTitleRequired,
                    "Questionnaire's title can not be empty or contains whitespaces only");
            }
        }

        private void ThrowDomainExceptionIfQuestionDoesNotExist(Guid publicKey)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(publicKey);
            if (question == null)
            {
                throw new DomainException(
                    DomainExceptionType.QuestionNotFound,
                    string.Format("Question with public key {0} can't be found", publicKey));
            }
        }

        private void ThrowDomainExceptionIfGroupDoesNotExist(Guid groupPublicKey)
        {
            var group = this.innerDocument.Find<Group>(groupPublicKey);
            if (group == null)
            {
                throw new DomainException(
                    DomainExceptionType.GroupNotFound,
                    string.Format("group with public key {0} can't be found", groupPublicKey));
            }
        }
        
        private void ThrowDomainExceptionIfTitleIsEmpty(string title)
        {
           if(string.IsNullOrEmpty(title))
               throw new DomainException(DomainExceptionType.QuestionTitleRequired, "Question title can't be empty");
        }

        private void ThrowDomainExceptionIfStataCaptionIsInvalid(Guid questionPublicKey, string stataCaption)
        {
            if (string.IsNullOrEmpty(stataCaption))
            {
                throw new DomainException(
                    DomainExceptionType.VariableNameRequired, "Variable name shouldn't be empty or contains white spaces");
            }

            bool isTooLong = stataCaption.Length > 32;
            if (isTooLong)
            {
                throw new DomainException(
                    DomainExceptionType.VariableNameMaxLength, "Variable name shouldn't be longer than 32 characters");
            }

            bool containsInvalidCharacters = stataCaption.Any(c => !(c == '_' || Char.IsLetterOrDigit(c)));
            if (containsInvalidCharacters)
            {
                throw new DomainException(
                    DomainExceptionType.VariableNameSpecialCharacters,
                    "Valid variable name should contains only letters, digits and underscore character");
            }

            bool startsWithDigit = Char.IsDigit(stataCaption[0]);
            if (startsWithDigit)
            {
                throw new DomainException(
                    DomainExceptionType.VariableNameStartWithDigit, "Variable name shouldn't starts with digit");
            }

            var captions = this.innerDocument.GetAllQuestions<AbstractQuestion>()
                               .Where(q => q.PublicKey != questionPublicKey)
                               .Select(q => q.StataExportCaption);

            bool isNotUnique = captions.Contains(stataCaption);
            if (isNotUnique)
            {
                throw new DomainException(
                   DomainExceptionType.VarialbeNameNotUnique, "Variable name should be unique in questionnaire's scope");
            }
        }

        private void ThrowDomainExceptionIfQuestionTypeIsNotAllowed(QuestionType type)
        {
            bool isQuestionTypeAllowed = AllowedQuestionTypes.Contains(type);

            if (!isQuestionTypeAllowed)
                throw new DomainException(DomainExceptionType.NotAllowedQuestionType, 
                    string.Format("Question type {0} is not allowed", type));
        }

        private void ThrowDomainExceptionIfQuestionWithOptionsIsInvalid(
            QuestionType questionType, Option[] options)
        {
            bool isQuestionWithOptions = questionType == QuestionType.MultyOption || questionType == QuestionType.SingleOption;
            if (!isQuestionWithOptions)
                return;

            if (!options.Any())
            {
                throw new DomainException(
                    DomainExceptionType.SelectorEmpty, "Question with options should have one option at least");
            }

            if (options.Any(x => string.IsNullOrEmpty(x.Value)))
            {
                throw new DomainException(
                    DomainExceptionType.SelectorValueRequired, "Answer option value is required");
            }

            if (options.Any(x => !x.Value.IsInteger()))
            {
                throw new DomainException(
                    DomainExceptionType.SelectorValueSpecialCharacters,
                    "Answer option value should have only number characters");
            }

            if (!AreElementsUnique(options.Select(x => x.Value)))
            {
                throw new DomainException(
                    DomainExceptionType.SelectorValueNotUnique,
                    "Answer option value should have unique in options scope");
            }

            if (options.Any(x => string.IsNullOrEmpty(x.Title)))
            {
                throw new DomainException(DomainExceptionType.SelectorTextRequired, "Answer title can't be empty");
            }

            if (!AreElementsUnique(options.Select(x => x.Title)))
            {
                throw new DomainException(DomainExceptionType.SelectorTextNotUnique, "Answer title is not unique");
            }
        }

        private static bool AreElementsUnique(IEnumerable<string> elements)
        {
            return elements.Distinct().Count() == elements.Count();
        }
    }
}