using Main.Core.Domain.Exceptions;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Aggregates.Snapshots;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.GenericSubdomains.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.Utility;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;

namespace WB.Core.BoundedContexts.Designer.Aggregates
{
    using Main.Core.Entities;

    public class Questionnaire : AggregateRootMappedByConvention, ISnapshotable<QuestionnaireState>
    {
        
        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();

        private readonly IQuestionFactory questionFactory;

        private readonly ILogger logger;

        private static readonly HashSet<QuestionType> AllowedQuestionTypes = new HashSet<QuestionType>
        {
            QuestionType.SingleOption,
            QuestionType.MultyOption,
            QuestionType.Numeric,
            QuestionType.DateTime,
            QuestionType.Text,
            QuestionType.AutoPropagate,
            QuestionType.GpsCoordinates
        };

        private static readonly HashSet<QuestionType> ReroutedQuestionTypes = new HashSet<QuestionType>
        {
            QuestionType.Numeric,
            QuestionType.AutoPropagate
        };
        
        private static readonly int maxCountOfDecimaPlaces = 15;

        public Questionnaire()
            : base()
        {
            this.questionFactory = new QuestionFactory();
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
        }

      
        public Questionnaire(Guid publicKey)
            : base(publicKey)
        {
            this.questionFactory = new QuestionFactory();
        }

        public Questionnaire(Guid publicKey, string title, Guid? createdBy = null, bool isPublic = false)
            : base(publicKey)
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespaces(title);

            var clock = NcqrsEnvironment.Get<IClock>();
            this.questionFactory = new QuestionFactory();

            this.ApplyEvent(
                new NewQuestionnaireCreated
                    {
                        IsPublic = isPublic,
                        PublicKey = publicKey,
                        Title = title,
                        CreationDate = clock.UtcNow(),
                        CreatedBy = createdBy
                    });
        }
        
        public  Questionnaire(Guid createdBy, IQuestionnaireDocument source): base(source.PublicKey)
        {
            ImportQuestionnaire(createdBy, source);
        }

        public Questionnaire(Guid publicKey, string title, Guid createdBy, IQuestionnaireDocument source) : this(publicKey, title,createdBy,false,source)
        {}

        public Questionnaire(Guid publicKey, string title, Guid createdBy, bool isPublic, IQuestionnaireDocument source)
            : base(publicKey)
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespaces(title);

            var clock = NcqrsEnvironment.Get<IClock>();

            var document = source as QuestionnaireDocument;
            if (document == null)
                throw new DomainException(DomainExceptionType.TemplateIsInvalid, "only QuestionnaireDocuments are supported for now");

            var clonedDocument = (QuestionnaireDocument)document.Clone();
            clonedDocument.PublicKey = this.EventSourceId;
            clonedDocument.CreatedBy = createdBy;
            clonedDocument.CreationDate = clock.UtcNow();
            clonedDocument.Title = title;
            clonedDocument.IsPublic = isPublic;
            if (clonedDocument.SharedPersons != null)
            {
                clonedDocument.SharedPersons.Clear();
            }

            ApplyEvent(new QuestionnaireCloned
            {
                QuestionnaireDocument = clonedDocument,
                ClonedFromQuestionnaireId = clonedDocument.PublicKey,
                ClonedFromQuestionnaireVersion = clonedDocument.LastEventSequence
            });
        }

        public QuestionnaireState CreateSnapshot()
        {
            return new QuestionnaireState
            {
                QuestionnaireDocument = this.innerDocument,
                Version = this.Version
            };
        }
        public void RestoreFromSnapshot(QuestionnaireState snapshot)
       {
            this.innerDocument = snapshot.QuestionnaireDocument.Clone() as QuestionnaireDocument;
        }

        public void ImportQuestionnaire(Guid createdBy, IQuestionnaireDocument source)
        {
           
            var document = source as QuestionnaireDocument;
            if (document == null)
                throw new DomainException(DomainExceptionType.TemplateIsInvalid, "Only QuestionnaireDocuments are supported for now");
            document.CreatedBy = createdBy;
            ApplyEvent(new TemplateImported() {Source = document});
           
        }


        public void UpdateQuestionnaire(string title, bool isPublic, Guid responsibleId)
#warning CRUD
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespaces(title);

            this.ApplyEvent(new QuestionnaireUpdated() {Title = title, IsPublic = isPublic, ResponsibleId = responsibleId});
        }

        public void DeleteQuestionnaire()
        {
            this.ApplyEvent(new QuestionnaireDeleted());
        }


        public void DeleteImage(Guid questionKey, Guid imageKey, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            this.ApplyEvent(new ImageDeleted { ImageKey = imageKey, QuestionKey = questionKey, ResponsibleId = responsibleId });
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
            Guid? parentGroupId, string title, Propagate propagationKind, string description, string condition, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupAlreadyExists(groupId);
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
                ResponsibleId = responsibleId
            });
        }


        public void CloneGroupWithoutChildren(Guid groupId,
            Guid? parentGroupId, string title, Propagate propagationKind, string description, string condition, Guid sourceGroupId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupAlreadyExists(groupId);
            this.ThrowDomainExceptionIfParentGroupCantHaveChildGroups(parentGroupId);

            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespaces(title);

            this.ThrowDomainExceptionIfGroupsPropagationKindIsNotSupported(propagationKind);

            this.ApplyEvent(new GroupCloned
            {
                PublicKey = groupId,
                GroupText = title,
                ParentGroupPublicKey = parentGroupId,
                Paropagateble = propagationKind,
                Description = description,
                ConditionExpression = condition,
                SourceGroupId = sourceGroupId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            });
        }


        public void NewDeleteGroup(Guid groupId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);
            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            this.ApplyEvent(new GroupDeleted(){GroupPublicKey = groupId, ResponsibleId = responsibleId});   
        }

        public void MoveGroup(Guid groupId, Guid? targetGroupId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);

            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            if (targetGroupId.HasValue)
            {
                this.ThrowDomainExceptionIfGroupDoesNotExist(targetGroupId.Value);

                this.ThrowDomainExceptionIfTargetGroupCannotHaveChildGroups(targetGroupId.Value);
            }

            this.ApplyEvent(new QuestionnaireItemMoved
            {
                PublicKey = groupId,
                GroupKey = targetGroupId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            });
        }

        public void NewUpdateGroup(Guid groupId, string title, Propagate propagationKind, string description, string condition, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);

            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespaces(title);

            this.ThrowDomainExceptionIfGroupsPropagationKindIsNotSupported(propagationKind);

            this.ThrowDomainExceptionIfGroupsPropagationKindCannotBeChanged(groupId, propagationKind);

            this.ApplyEvent(new GroupUpdated
            {
                QuestionnaireId = this.innerDocument.PublicKey.ToString(),
                GroupPublicKey = groupId,
                GroupText = title,
                Propagateble = propagationKind,
                Description = description,
                ConditionExpression = condition,
                ResponsibleId = responsibleId
            });
        }

        public void CloneQuestion(Guid questionId,
            Guid groupId, string title, QuestionType type, string alias,
            bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage,
            string instructions, Option[] options, Order optionsOrder, Guid sourceQuestionId, int targetIndex, Guid responsibleId, 
            Guid? linkedToQuestionId, bool areAnswersOrdered, int? maxAllowedAnswers)
        {
            this.ThrowDomainExceptionIfQuestionTypeIsReroutedOnQuestionTypeSpecificCommand(type); 
            this.ThrowDomainExceptionIfQuestionAlreadyExists(questionId);

            alias = alias.Trim();
            title = title.Trim();

            var parentGroup = this.innerDocument.Find<IGroup>(groupId);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, type, alias, isFeatured, isHeaderOfPropagatableGroup, validationExpression, responsibleId);

            this.ThrowIfNotCategoricalQuestionHasLinkedInformation(type, linkedToQuestionId);
            this.ThrowIfQuestionIsCategoricalAndInvalid(type, options, linkedToQuestionId, isFeatured, isHeaderOfPropagatableGroup);

            this.ThrowIfMaxAllowedAnswersInvalid(type, linkedToQuestionId, maxAllowedAnswers, options);
            

            this.ApplyEvent(new QuestionCloned
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
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId,
                LinkedToQuestionId = linkedToQuestionId,

                AreAnswersOrdered = areAnswersOrdered,
                MaxAllowedAnswers = maxAllowedAnswers
            });
        }

        public void CloneNumericQuestion(Guid questionId,
            Guid groupId, string title, bool isAutopropagating, string alias,
            bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage,
            string instructions, Guid sourceQuestionId, int targetIndex, Guid responsibleId, int? maxValue, Guid[] triggeredGroupIds,
            bool isInteger, int? countOfDecimalPlaces)
        {
            this.ThrowDomainExceptionIfQuestionAlreadyExists(questionId);

            alias = alias.Trim();
            title = title.Trim();

            var parentGroup = this.innerDocument.Find<IGroup>(groupId);

            var questionType = NumericQuestionUtils.GetQuestionTypeFromIsAutopropagatingParameter(isAutopropagating);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, questionType, alias, isFeatured, isHeaderOfPropagatableGroup, validationExpression, responsibleId);

            this.ThrowIfPrecisionSettingsAreInConflictWithPropagationSettings(isAutopropagating, isInteger);
            this.ThrowIfPrecisionSettingsAreInConflictWithDecimalPlaces(isInteger, countOfDecimalPlaces);
            this.ThrowIfDecimalPlacesValueIsIncorrect(countOfDecimalPlaces);
            this.ThrowDomainExceptionIfAnyTriggerLinksToAbsentOrNotPropagatedGroup(isAutopropagating, triggeredGroupIds);


            this.ApplyEvent(new NumericQuestionCloned
            {
                PublicKey = questionId,
                GroupPublicKey = groupId,
                QuestionText = title,
                IsAutopropagating = isAutopropagating,
                StataExportCaption = alias,

                Mandatory = isMandatory,
                Featured = isFeatured,
                Capital = isHeaderOfPropagatableGroup,

                QuestionScope = scope,
                ConditionExpression = condition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId,
                MaxValue = maxValue ?? 10,
                Triggers = triggeredGroupIds != null ? triggeredGroupIds.ToList() : null,
                IsInteger = isInteger,
                CountOfDecimalPlaces = countOfDecimalPlaces
            });
        }

        public void NewAddQuestion(Guid questionId,
           Guid groupId, string title, QuestionType type, string alias,
           bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
           QuestionScope scope, string condition, string validationExpression, string validationMessage,
           string instructions, Option[] options, Order optionsOrder, Guid responsibleId, Guid? linkedToQuestionId, bool areAnswersOrdered, int? maxAllowedAnswers)
        {
            this.ThrowDomainExceptionIfQuestionTypeIsReroutedOnQuestionTypeSpecificCommand(type);
            this.ThrowDomainExceptionIfQuestionAlreadyExists(questionId);

            alias = alias.Trim();
            title = title.Trim();
            var parentGroup = this.innerDocument.Find<IGroup>(groupId);
            
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, type, alias, 
                isFeatured, isHeaderOfPropagatableGroup, validationExpression, responsibleId);

            this.ThrowIfNotCategoricalQuestionHasLinkedInformation(type, linkedToQuestionId);
            this.ThrowIfQuestionIsCategoricalAndInvalid(type, options, linkedToQuestionId, isFeatured, isHeaderOfPropagatableGroup);

            this.ThrowIfMaxAllowedAnswersInvalid(type, linkedToQuestionId, maxAllowedAnswers, options);

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
                ResponsibleId = responsibleId,
                LinkedToQuestionId = linkedToQuestionId,

                AreAnswersOrdered = areAnswersOrdered,
                MaxAllowedAnswers = maxAllowedAnswers
            });
        }

        public void AddNumericQuestion(Guid questionId,
            Guid groupId, string title, bool isAutopropagating, string alias,
            bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage,
            string instructions, int? maxValue, Guid[] triggeredGroupIds, Guid responsibleId,
            bool isInteger, int? countOfDecimalPlaces)
        {
            this.ThrowDomainExceptionIfQuestionAlreadyExists(questionId);
            alias = alias.Trim();
            title = title.Trim();

            var parentGroup = this.innerDocument.Find<IGroup>(groupId);
            var questionType = NumericQuestionUtils.GetQuestionTypeFromIsAutopropagatingParameter(isAutopropagating);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, questionType, 
                alias, isFeatured, isHeaderOfPropagatableGroup, validationExpression, responsibleId);
            this.ThrowIfPrecisionSettingsAreInConflictWithPropagationSettings(isAutopropagating, isInteger);
            this.ThrowIfPrecisionSettingsAreInConflictWithDecimalPlaces(isInteger,countOfDecimalPlaces);
            this.ThrowIfDecimalPlacesValueIsIncorrect(countOfDecimalPlaces);
            this.ThrowDomainExceptionIfAnyTriggerLinksToAbsentOrNotPropagatedGroup(isAutopropagating, triggeredGroupIds);

            this.ApplyEvent(new NumericQuestionAdded
            {
                PublicKey = questionId,
                GroupPublicKey = groupId,
                QuestionText = title,
                IsAutopropagating = isAutopropagating,
                StataExportCaption = alias,
                Mandatory = isMandatory,
                Featured = isFeatured,
                Capital = isHeaderOfPropagatableGroup,
                QuestionScope = scope,
                ConditionExpression = condition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                ResponsibleId = responsibleId,


                MaxValue = maxValue ?? 10,
                Triggers = triggeredGroupIds != null ? triggeredGroupIds.ToList() : null,
                IsInteger = isInteger,
                CountOfDecimalPlaces = countOfDecimalPlaces
            });
        }

        private void ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(Guid questionId, IGroup parentGroup, string title, QuestionType type,
            string alias, bool isFeatured, bool isHeaderOfPropagatableGroup, string validationExpression, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfQuestionTypeIsNotAllowed(type);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfTitleIsEmpty(title);
            this.ThrowDomainExceptionIfVariableNameIsInvalid(questionId, alias);
            this.ThrowDomainExceptionIfQuestionCanNotBeFeatured(type, isFeatured);
            this.ThrowDomainExceptionIfQuestionCanNotContainValidations(type, validationExpression);


            this.ThrowDomainExceptionIfQuestionIsFeaturedButGroupIsPropagated(isFeatured, parentGroup);
            this.ThrowDomainExceptionIfQuestionIsHeadOfGroupButGroupIsNotPropagated(isHeaderOfPropagatableGroup, parentGroup);

            this.ThrowDomainExceptionIfQuestionTitleContainsIncorrectSubstitution(title, alias, questionId, isFeatured, parentGroup);
        }

        public void NewDeleteQuestion(Guid questionId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            this.ApplyEvent(new QuestionDeleted() {QuestionId = questionId, ResponsibleId = responsibleId});
        }

        public void MoveQuestion(Guid questionId, Guid targetGroupId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(targetGroupId);
            
            #warning reorganize checkings - now we are searching for items several times
            
            var question = this.innerDocument.Find<AbstractQuestion>(questionId);
            var parentGroup = this.innerDocument.Find<IGroup>(targetGroupId);
            this.ThrowDomainExceptionIfQuestionTitleContainsIncorrectSubstitution(question.QuestionText, question.StataExportCaption, questionId, question.Featured, parentGroup);

            this.ApplyEvent(new QuestionnaireItemMoved
            {
                PublicKey = questionId,
                GroupKey = targetGroupId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            });
        }

        public void NewUpdateQuestion(Guid questionId,
            string title, QuestionType type, string alias,
            bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage,
            string instructions, Option[] options, Order optionsOrder, Guid responsibleId, Guid? linkedToQuestionId, 
            bool areAnswersOrdered, int? maxAllowedAnswers)
        {
            this.ThrowDomainExceptionIfQuestionTypeIsReroutedOnQuestionTypeSpecificCommand(type);
            
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            alias = alias.Trim();
            title = title.Trim();
            
            IGroup parentGroup = this.innerDocument.GetParentOfQuestion(questionId);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, type, alias, isFeatured, isHeaderOfPropagatableGroup, validationExpression, responsibleId);
            
            this.ThrowIfNotCategoricalQuestionHasLinkedInformation(type, linkedToQuestionId);
            this.ThrowIfQuestionIsCategoricalAndInvalid(type, options, linkedToQuestionId, isFeatured, isHeaderOfPropagatableGroup);
            this.ThrowIfMaxAllowedAnswersInvalid(type, linkedToQuestionId, maxAllowedAnswers, options);

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
                ResponsibleId = responsibleId,
                LinkedToQuestionId = linkedToQuestionId,

                AreAnswersOrdered = areAnswersOrdered,
                MaxAllowedAnswers = maxAllowedAnswers
            });
        }

        public void UpdateNumericQuestion(Guid questionId,
            string title, bool isAutopropagating, string alias,
            bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage,
            string instructions, int? maxValue, Guid[] triggeredGroupIds, Guid responsibleId, bool isInteger, int? countOfDecimalPlaces)
        {
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            alias = alias.Trim();
            title = title.Trim();

            IGroup parentGroup = this.innerDocument.GetParentOfQuestion(questionId);
            var questionType = NumericQuestionUtils.GetQuestionTypeFromIsAutopropagatingParameter(isAutopropagating);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, questionType, alias, isFeatured, 
                                                                         isHeaderOfPropagatableGroup, validationExpression, responsibleId);

            this.ThrowIfPrecisionSettingsAreInConflictWithPropagationSettings(isAutopropagating, isInteger);
            this.ThrowIfPrecisionSettingsAreInConflictWithDecimalPlaces(isInteger, countOfDecimalPlaces);
            this.ThrowIfDecimalPlacesValueIsIncorrect(countOfDecimalPlaces);
            this.ThrowDomainExceptionIfAnyTriggerLinksToAbsentOrNotPropagatedGroup(isAutopropagating, triggeredGroupIds);

            this.ApplyEvent(new NumericQuestionChanged
            {
                PublicKey = questionId,
                QuestionText = title,
                IsAutopropagating = isAutopropagating,
                StataExportCaption = alias,
                Mandatory = isMandatory,
                Featured = isFeatured,
                Capital = isHeaderOfPropagatableGroup,
                QuestionScope = scope,
                ConditionExpression = condition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                ResponsibleId = responsibleId,


                MaxValue = maxValue ?? 10,
                Triggers = triggeredGroupIds != null ? triggeredGroupIds.ToList() : null,
                IsInteger = isInteger,
                CountOfDecimalPlaces = countOfDecimalPlaces
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

        public void AddSharedPerson(Guid personId, string email, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            if (responsibleId == personId)
            {
                throw new DomainException(
                    DomainExceptionType.OwnerCannotBeInShareList,
                    "You are the owner of this questionnaire. Please, input another email");
            }

            if (this.innerDocument.SharedPersons.Contains(personId))
            {
                throw new DomainException(
                    DomainExceptionType.UserExistInShareList,
                    string.Format("User {0} already exist in share list", email));
            }

            this.ApplyEvent(new SharedPersonToQuestionnaireAdded()
            {
                PersonId = personId,
                Email = email,
                ResponsibleId = responsibleId
            });
        }

        public void RemoveSharedPerson(Guid personId, string email, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            if (!this.innerDocument.SharedPersons.Contains(personId))
            {
                throw new DomainException(
                   DomainExceptionType.UserDoesNotExistInShareList,
                   "Couldn't remove user, because it doesn't exist in share list");
            }

            this.ApplyEvent(new SharedPersonFromQuestionnaireRemoved()
            {
                PersonId = personId,
                ResponsibleId = responsibleId
            });
        }

        protected void OnSharedPersonToQuestionnaireAdded(SharedPersonToQuestionnaireAdded e)
        {
            this.innerDocument.SharedPersons.Add(e.PersonId);
        }

        protected void OnSharedPersonFromQuestionnaireRemoved(SharedPersonFromQuestionnaireRemoved e)
        {
            this.innerDocument.SharedPersons.Remove(e.PersonId);
        }

        protected void OnQuestionnaireUpdated(QuestionnaireUpdated e)
        {
            this.innerDocument.Title = e.Title;
            this.innerDocument.IsPublic = e.IsPublic;
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

        protected internal void OnNewGroupAdded(NewGroupAdded e)
        {
            var group = new Group();
            group.Title = e.GroupText;
            group.Propagated = e.Paropagateble;
            group.PublicKey = e.PublicKey;
            group.Description = e.Description;
            group.ConditionExpression = e.ConditionExpression;
            this.innerDocument.Add(group, e.ParentGroupPublicKey, null);
        }

        protected internal void OnTemplateImported(TemplateImported e)
        {
            this.innerDocument = e.Source;
        }

        protected internal void OnQuestionnaireCloned(QuestionnaireCloned e)
        {
            this.innerDocument = e.QuestionnaireDocument;
        }

        protected internal void OnGroupCloned(GroupCloned e)
        {
            var group = new Group();
            group.Title = e.GroupText;
            group.Propagated = e.Paropagateble;
            group.PublicKey = e.PublicKey;
            group.Description = e.Description;
            group.ConditionExpression = e.ConditionExpression;
            this.innerDocument.Insert(e.TargetIndex, group, e.ParentGroupPublicKey);
        }


        protected internal void OnNewQuestionAdded(NewQuestionAdded e)
        {
            AbstractQuestion question =
                new QuestionFactory().CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        e.AnswerOrder,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Triggers,
                        e.MaxValue,
                        e.Answers,
                        e.LinkedToQuestionId,
                        e.IsInteger,
                        null,
                        e.AreAnswersOrdered,
                        e.MaxAllowedAnswers));
            if (question == null)
            {
                return;
            }

            this.innerDocument.Add(question, e.GroupPublicKey, null);
        }

        protected internal void OnNumericQuestionAdded(NumericQuestionAdded e)
        {
            AbstractQuestion question =
                new QuestionFactory().CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        NumericQuestionUtils.GetQuestionTypeFromIsAutopropagatingParameter(e.IsAutopropagating),
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        Order.AZ, 
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Triggers,
                        e.MaxValue,
                        null,
                        null,
                        e.IsInteger,
                        e.CountOfDecimalPlaces,
                        null,
                        null));
            if (question == null)
            {
                return;
            }

            this.innerDocument.Add(question, e.GroupPublicKey, null);
        }

        protected internal void OnQuestionCloned(QuestionCloned e)
        {
            AbstractQuestion question =
                new QuestionFactory().CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        e.AnswerOrder,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Triggers,
                        e.MaxValue,
                        e.Answers,
                        e.LinkedToQuestionId,
                        e.IsInteger,
                        null,
                        e.AreAnswersOrdered,
                        e.MaxAllowedAnswers));
            if (question == null)
            {
                return;
            }

            this.innerDocument.Insert(e.TargetIndex, question, e.GroupPublicKey);
        }

        protected internal void OnNumericQuestionCloned(NumericQuestionCloned e)
        {
            AbstractQuestion question =
                new QuestionFactory().CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        NumericQuestionUtils.GetQuestionTypeFromIsAutopropagatingParameter(e.IsAutopropagating),
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        Order.AZ, 
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Triggers,
                        e.MaxValue,
                        null,
                        null,
                        e.IsInteger, 
                        e.CountOfDecimalPlaces,
                        null,
                        null));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Insert(e.TargetIndex, question, e.GroupPublicKey);
        }

        protected void OnNewQuestionnaireCreated(NewQuestionnaireCreated e)
        {
            this.innerDocument.IsPublic = e.IsPublic;
            this.innerDocument.Title = e.Title;
            this.innerDocument.PublicKey = e.PublicKey;
            this.innerDocument.CreationDate = e.CreationDate;
            this.innerDocument.LastEntryDate = e.CreationDate;
            this.innerDocument.CreatedBy = e.CreatedBy;
        }

        protected void OnQuestionChanged(QuestionChanged e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);
            IQuestion newQuestion =
                this.questionFactory.CreateQuestion(
                    new QuestionData(
                        question.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        e.AnswerOrder,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Triggers,
                        e.MaxValue,
                        e.Answers,
                        e.LinkedToQuestionId,
                        e.IsInteger,
                        null,
                        e.AreAnswersOrdered,
                        e.MaxAllowedAnswers));
            this.innerDocument.ReplaceQuestionWithNew(question, newQuestion);
        }

        protected void OnNumericQuestionChanged(NumericQuestionChanged e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);
            IQuestion newQuestion =
                this.questionFactory.CreateQuestion(
                    new QuestionData(
                        question.PublicKey,
                        NumericQuestionUtils.GetQuestionTypeFromIsAutopropagatingParameter(e.IsAutopropagating),
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        Order.AZ, 
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Triggers,
                        e.MaxValue,
                        null,
                        null,
                        e.IsInteger, 
                        e.CountOfDecimalPlaces,
                        null,
                        null));
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
                logger.Warn(string.Format("Ignored legacy MoveItem event in questionnaire {0}", this.EventSourceId));
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

        private void ThrowDomainExceptionIfGroupsPropagationKindCannotBeChanged(Guid groupId, Propagate newPropagationKind)
        {
            if (newPropagationKind == Propagate.None)
                return;

            var group = this.innerDocument.Find<Group>(groupId);

            bool hasAnyChildGroup = group.Children.Any(g => g is Group);
            if (hasAnyChildGroup)
            {
                throw new DomainException(DomainExceptionType.GroupCantBecomeAutoPropagateIfHasAnyChildGroup, "Auto propagated groups can't have child groups");
            }
        }

        private void ThrowDomainExceptionIfTargetGroupCannotHaveChildGroups(Guid groupId)
        {
            var group = this.innerDocument.Find<Group>(groupId);
            if (group.Propagated == Propagate.AutoPropagated)
            {
                throw new DomainException(DomainExceptionType.AutoPropagateGroupCantHaveChildGroups, "Auto propagated groups can't have child groups");
            }
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


        private void ThrowDomainExceptionIfAnyTriggerLinksToAbsentOrNotPropagatedGroup(bool isAutopropagating, Guid[] triggeredGroupIds)
        {
            bool noGroupsShouldBeTrigged = triggeredGroupIds == null || triggeredGroupIds.Length == 0;
            if (!isAutopropagating || noGroupsShouldBeTrigged)
                return;

            foreach (Guid groupId in triggeredGroupIds)
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
                    "Question inside propagated group can not be pre-filled");
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
                    "The titles of groups and chapters can not be empty or contains whitespace only");
            }
        }

        private void ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespaces(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new DomainException(
                    DomainExceptionType.QuestionnaireTitleRequired,
                    "Questionnaire's title can not be empty or contains whitespace only");
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
            if (string.IsNullOrEmpty(title))
                throw new DomainException(DomainExceptionType.QuestionTitleRequired, "Question title can't be empty");
        }

        private void ThrowDomainExceptionIfVariableNameIsInvalid(Guid questionPublicKey, string stataCaption)
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

            var keywords = new[] { "this" };
            foreach (var keyword in keywords)
            {
                if (stataCaption.ToLower() == keyword)
                {
                    throw new DomainException(
                        DomainExceptionType.VariableNameShouldNotMatchWithKeywords, keyword + " is a keyword. Variable name shouldn't match with keywords");
                }
            }
        }

        private void ThrowDomainExceptionIfQuestionTypeIsNotAllowed(QuestionType type)
        {
            bool isQuestionTypeAllowed = AllowedQuestionTypes.Contains(type);

            if (!isQuestionTypeAllowed)
                throw new DomainException(DomainExceptionType.NotAllowedQuestionType,
                    string.Format("Question type {0} is not allowed", type));
        }

        private void ThrowDomainExceptionIfQuestionTypeIsReroutedOnQuestionTypeSpecificCommand(QuestionType type)
        {
            bool isQuestionTypeRerouted = ReroutedQuestionTypes.Contains(type);

            if (isQuestionTypeRerouted)
                throw new DomainException(DomainExceptionType.QuestionTypeIsReroutedOnQuestionTypeSpecificCommand,
                    string.Format("Question type {0} rerouted on QuestionType specific command", type));
        }

        private bool IsUnderPropagatableGroup(IComposite item)
        {
            this.innerDocument.ConnectChildrenWithParent();

            return this.GetFirstPropagatableParentGroupIdOrNull(item) != null;
        }

        private Guid? GetFirstPropagatableParentGroupIdOrNull(IComposite item)
        {
            if (item == null)
                return null;

            var itemAsGroup = item as IGroup;
            if (itemAsGroup != null)
            {
                if (itemAsGroup.Propagated == Propagate.AutoPropagated)
                    return itemAsGroup.PublicKey;
            }
            
            return this.GetFirstPropagatableParentGroupIdOrNull(item.GetParent());
        }


        private List<Guid> GetAllAutopropagationQuestionsAsVector(IComposite item)
        {
            var allQuestion = new List<Guid>();
            var itemAsGroup = item;

            while (itemAsGroup != null)
            {
                IGroup @group = itemAsGroup as IGroup;
                if (@group.Propagated == Propagate.AutoPropagated)
                {
                    var autoPropagationQuestion =
                        this.innerDocument.Find<AutoPropagateQuestion>(
                            question => question.Triggers.Contains(@group.PublicKey)).FirstOrDefault();

                    if(autoPropagationQuestion != null)
                        allQuestion.Add(autoPropagationQuestion.PublicKey);
                }

                itemAsGroup = itemAsGroup.GetParent();
            }
            return allQuestion;
        }


        private void ThrowIfNotCategoricalQuestionHasLinkedInformation(QuestionType questionType, Guid? linkedToQuestionId)
        {
            bool isCategoricalQuestion = questionType == QuestionType.MultyOption ||
                questionType == QuestionType.SingleOption;

            bool notCategoricalQuestionHasLinkedQuestionId = linkedToQuestionId.HasValue && !isCategoricalQuestion;

            if (notCategoricalQuestionHasLinkedQuestionId)
            {
                throw new DomainException(
                    DomainExceptionType.NotCategoricalQuestionLinkedToAnoterQuestion,
                    "Only categorical question can be linked to another question");
            }
        }

        private void ThrowIfQuestionIsCategoricalAndInvalid(QuestionType questionType, Option[] options, Guid? linkedToQuestionId, bool isFeatured,
            bool isHead)
        {
            bool isCategoricalQuestion =
                questionType == QuestionType.MultyOption ||
                questionType == QuestionType.SingleOption;

            if (!isCategoricalQuestion)
                return;

            bool questionIsLinked = linkedToQuestionId.HasValue;
            bool questionHasOptions = options != null && options.Any();

            if (questionIsLinked && questionHasOptions)
            {
                throw new DomainException(
                    DomainExceptionType.ConflictBetweenLinkedQuestionAndOptions,
                    "Categorical question cannot be with answers and linked to another question in the same time");
            }

            if (questionIsLinked)
            {
                this.ThrowIfLinkedCategoricalQuestionIsInvalid(linkedToQuestionId, isFeatured, isHead);
            }
            else
            {
                ThrowIfNotLinkedCategoricalQuestionIsInvalid(options);
            }

            if (questionType == QuestionType.MultyOption && isFeatured)
            {
                throw new DomainException(
                    DomainExceptionType.MultiOptionQuestionCanNotBeFeatured,
                    "Multiple answers question can not be pre-filled");
            }
        }

        private void ThrowIfMaxAllowedAnswersInvalid(QuestionType questionType, Guid? linkedToQuestionId, int? maxAllowedAnswers, Option[] options)
        {
            if (questionType != QuestionType.MultyOption) return;
            if (!maxAllowedAnswers.HasValue) return;

            if (maxAllowedAnswers.Value < 1)
            {
                throw new DomainException(
                    DomainExceptionType.MaxAllowedAnswersIsNotPositive,
                    "Maximum Allowed Answers for question has to be positive");
            }

            if (!linkedToQuestionId.HasValue && maxAllowedAnswers.Value > options.Length)
            {
                throw new DomainException(
                    DomainExceptionType.MaxAllowedAnswersMoreThanOptions,
                    "Maximum Allowed Answers more than question's options");
            }
        }

        private void ThrowIfLinkedCategoricalQuestionIsInvalid(Guid? linkedToQuestionId, bool isFeatured, bool isHead)
        {
            var linkedToQuestion =
                this.innerDocument.Find<IQuestion>(x => x.PublicKey == linkedToQuestionId).FirstOrDefault();

            if (linkedToQuestion == null)
            {
                throw new DomainException(
                    DomainExceptionType.LinkedQuestionDoesNotExist,
                    "Question that you are linked to does not exist");
            }

            bool typeOfLinkedQuestionIsNotSupported = !(
                linkedToQuestion.QuestionType == QuestionType.DateTime ||
                linkedToQuestion.QuestionType == QuestionType.Numeric ||
                linkedToQuestion.QuestionType == QuestionType.Text);

            if (typeOfLinkedQuestionIsNotSupported)
            {
                throw new DomainException(
                    DomainExceptionType.NotSupportedQuestionForLinkedQuestion,
                    "Linked question can be only type of number, text or date");
            }

            if (isFeatured)
            {
                throw new DomainException(
                    DomainExceptionType.QuestionWithLinkedQuestionCanNotBeFeatured,
                    "Question that linked to another question can not be pre-filled");
            }

            if (isHead)
            {
                throw new DomainException(
                    DomainExceptionType.QuestionWithLinkedQuestionCanNotBeHead,
                    "Question that linked to another question can not be head");
            }

            if (!this.IsUnderPropagatableGroup(linkedToQuestion))
            {
                throw new DomainException(
                    DomainExceptionType.LinkedQuestionIsNotInPropagateGroup,
                    "Question that you are linked to is not in the propagated group");
            }
        }

        private static void ThrowIfNotLinkedCategoricalQuestionIsInvalid(Option[] options)
        {
            if (options == null || !options.Any())
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

        private void ThrowDomainExceptionIfQuestionAlreadyExists(Guid questionId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IQuestion>(
                elementId: questionId,
                expectedCount: 0,
                exceptionType: DomainExceptionType.QuestionWithSuchIdAlreadyExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more question(s) with same ID {0} already exist:{1}{2}",
                        questionId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(question => question.QuestionText ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfGroupAlreadyExists(Guid groupId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IGroup>(
                elementId: groupId,
                expectedCount: 0,
                exceptionType: DomainExceptionType.GroupWithSuchIdAlreadyExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more group(s) with same ID {0} already exist:{1}{2}",
                        groupId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(group => group.Title ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfMoreThanOneQuestionExists(Guid questionId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IQuestion>(
                elementId: questionId,
                expectedCount: 1,
                exceptionType: DomainExceptionType.MoreThanOneQuestionsWithSuchIdExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more question(s) with same ID {0} already exist:{1}{2}",
                        questionId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(question => question.QuestionText ?? "<untitled>"))));
        }

        private void ThrowIfPrecisionSettingsAreInConflictWithPropagationSettings(bool isAutoPropagateQuestion, bool isInteger)
        {
            if (isAutoPropagateQuestion)
            {
                if (!isInteger)
                    throw new DomainException(
                    DomainExceptionType.AutoPropagateQuestionCantBeReal,
                    "AutoPropagate question can't be real");
            }
        }

        private void ThrowIfPrecisionSettingsAreInConflictWithDecimalPlaces(bool isInteger, int? countOfDecimalPlaces)
        {
            if (isInteger && countOfDecimalPlaces.HasValue)
            {
                    throw new DomainException(
                    DomainExceptionType.IntegerQuestionCantHaveDecimalPlacesSettings,
                    "AutoPropagate question can't have decimal places settings");
            }
        }

        private void ThrowIfDecimalPlacesValueIsIncorrect(int? countOfDecimalPlaces)
        {
            if(!countOfDecimalPlaces.HasValue)
                return;

            if (countOfDecimalPlaces.Value > maxCountOfDecimaPlaces)
            {
                throw new DomainException(
                    DomainExceptionType.CountOfDecimalPlacesValueIsIncorrect,
                    string.Format("Count of decimal places '{0}' exceeded maximum '{1}'.", countOfDecimalPlaces, maxCountOfDecimaPlaces));
            }

            if (countOfDecimalPlaces.Value < 0)
            {
                throw new DomainException(
                    DomainExceptionType.CountOfDecimalPlacesValueIsIncorrect,
                    string.Format("Count of decimal places cant be negative."));
            }

            if (countOfDecimalPlaces.Value == 0)
            {
                throw new DomainException(
                    DomainExceptionType.CountOfDecimalPlacesValueIsIncorrect,
                    string.Format("If you want Count of decimal places equals to 0 please select integer."));
            }
        }

        private void ThrowDomainExceptionIfMoreThanOneGroupExists(Guid groupId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IGroup>(
                elementId: groupId,
                expectedCount: 1,
                exceptionType: DomainExceptionType.MoreThanOneGroupsWithSuchIdExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more group(s) with same ID {0} already exist:{1}{2}",
                        groupId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(group => group.Title ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfElementCountIsMoreThanExpected<T>(Guid elementId, int expectedCount,
            DomainExceptionType exceptionType, Func<IEnumerable<T>, string> getExceptionDescription)
            where T : class, IComposite
        {
            List<T> elementsWithSameId = this.innerDocument.Find<T>(element => element.PublicKey == elementId).ToList();

            if (elementsWithSameId.Count > expectedCount)
            {
                throw new DomainException(exceptionType, getExceptionDescription(elementsWithSameId));
            }
        }

        private void ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(Guid viewerId)
        {
            if(this.innerDocument.CreatedBy != viewerId && !this.innerDocument.SharedPersons.Contains(viewerId))
            {
                throw new DomainException(
                    DomainExceptionType.DoesNotHavePermissionsForEdit,
                    "You don't have permissions for changing this questionnaire");
            }
        }

        private void ThrowDomainExceptionIfQuestionCanNotBeFeatured(QuestionType questionType, bool isFeatured)
        {
            if (isFeatured && questionType == QuestionType.GpsCoordinates)
                throw new DomainException(
                    DomainExceptionType.QuestionCanNotBeFeatured,
                    "Question can't be pre-filled");
        }

        private void ThrowDomainExceptionIfQuestionCanNotContainValidations(QuestionType questionType, string validationExpression)
        {
            if (!String.IsNullOrEmpty(validationExpression) && questionType == QuestionType.GpsCoordinates)
                throw new DomainException(
                    DomainExceptionType.QuestionCanNotContainValidation,
                    "Question cannot contain validations");
        }

        private void ThrowDomainExceptionIfQuestionTitleContainsIncorrectSubstitution(string questionTitle, string alias, Guid questionPublicKey, bool isFeatured, IGroup group)
        {
            string[] substitutionReferences = StringUtil.GetAllSubstitutionVariableNames(questionTitle);
            if(substitutionReferences.Length == 0)
                return;

            if (isFeatured)
                throw new DomainException(
                    DomainExceptionType.FeaturedQuestionTitleContainsSubstitutionReference,
                    "Pre-filled question title contains substitution references. It's illegal");

            if (substitutionReferences.Contains(alias))
                throw new DomainException(
                    DomainExceptionType.QuestionTitleContainsSubstitutionReferenceToSelf,
                    "Question title contains illegal substitution references to self");

            List<string> unknownReferences, questionsIncorrectTypeOfReferenced, questionsIllegalPropagationScope;

            this.innerDocument.ConnectChildrenWithParent(); //find all references and do it only once

            ValidateSubstitutionReferences(questionPublicKey, @group, substitutionReferences,
                out unknownReferences, out questionsIncorrectTypeOfReferenced, out questionsIllegalPropagationScope);

            if(unknownReferences.Count > 0)
                throw new DomainException(
                    DomainExceptionType.QuestionTitleContainsUnknownSubstitutionReference,
                    "Question title contains unknown substitution references: " + String.Join(", ", unknownReferences.ToArray()));

            if (questionsIncorrectTypeOfReferenced.Count > 0)
                throw new DomainException(
                    DomainExceptionType.QuestionTitleContainsSubstitutionReferenceQuestionOfInvalidType,
                    "Question title contains substitution references to questions of illegal type: " + String.Join(", ", questionsIncorrectTypeOfReferenced.ToArray()));

            if (questionsIllegalPropagationScope.Count > 0)
                throw new DomainException(
                    DomainExceptionType.QuestionTitleContainsInvalidSubstitutionReference,
                    "Question title contains illegal substitution references to questions: " + String.Join(", ", questionsIllegalPropagationScope.ToArray()));
        }

        private void ValidateSubstitutionReferences(Guid questionPublicKey, IGroup @group, string[] substitutionReferences,
            out List<string> unknownReferences, out List<string> questionsIncorrectTypeOfReferenced, out List<string> questionsIllegalPropagationScope)
        {
            unknownReferences = new List<string>();
            questionsIncorrectTypeOfReferenced = new List<string>();
            questionsIllegalPropagationScope = new List<string>();

            var questions = this.innerDocument.GetAllQuestions<AbstractQuestion>()
                                .Where(q => q.PublicKey != questionPublicKey)
                                .ToDictionary(q => q.StataExportCaption, q => q);

            List<Guid> propagationQuestionsVector = GetAllAutopropagationQuestionsAsVector(@group);

            foreach (var substitutionReference in substitutionReferences)
            {
                //extract validity of variable name to separate method and make check validity of substitutionReference  
                if (substitutionReference.Length > 32)
                {
                    unknownReferences.Add(substitutionReference);
                    continue;
                }

                if (!questions.ContainsKey(substitutionReference))
                    unknownReferences.Add(substitutionReference);
                else
                {
                    var currentQuestion = questions[substitutionReference];
                    bool typeOfRefQuestionIsNotSupported = !(currentQuestion.QuestionType == QuestionType.DateTime ||
                                                             currentQuestion.QuestionType == QuestionType.Numeric ||
                                                             currentQuestion.QuestionType == QuestionType.SingleOption ||
                                                             currentQuestion.QuestionType == QuestionType.Text ||
                                                             currentQuestion.QuestionType == QuestionType.AutoPropagate);

                    if (typeOfRefQuestionIsNotSupported)
                        questionsIncorrectTypeOfReferenced.Add(substitutionReference);

                    if (!IsReferencedSubstitutionLegal(propagationQuestionsVector, currentQuestion.GetParent()))
                        questionsIllegalPropagationScope.Add(substitutionReference);
                }
            }
        }

        private bool IsReferencedSubstitutionLegal(List<Guid> propagationQuestionsVector, IComposite referencedQuestionGroup)
        {
            List<Guid> referencedPropagationQuestionsVector = GetAllAutopropagationQuestionsAsVector(referencedQuestionGroup);

            if (referencedPropagationQuestionsVector.Count == 0) //referenced Question not in propagation - OK
                return true;

            if (propagationQuestionsVector.Count() < referencedPropagationQuestionsVector.Count())
                return false;

            return propagationQuestionsVector.Except(propagationQuestionsVector).Count() <= propagationQuestionsVector.Count() - referencedPropagationQuestionsVector.Count();
        }
    }
}