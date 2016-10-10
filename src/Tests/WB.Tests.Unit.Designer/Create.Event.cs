extern alias designer;
using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer
{
    internal static partial class Create
    {
        internal static class Event
        {
            public static NewGroupAdded AddGroup(Guid groupId, Guid? parentId = null, string variableName = null)
            {
                return new NewGroupAdded
                {
                    PublicKey = groupId,
                    ParentGroupPublicKey = parentId,
                    VariableName = variableName
                };
            }

            public static ExpressionsMigratedToCSharp ExpressionsMigratedToCSharpEvent()
            {
                return new ExpressionsMigratedToCSharp();
            }

            public static GroupBecameARoster GroupBecameRoster(Guid rosterId)
            {
                return new GroupBecameARoster(Guid.NewGuid(), rosterId);
            }
            
            /*public static NewQuestionAdded NewQuestionAdded(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
            string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string variableLabel = null, string validationExpression = null, string validationMessage = null,
            QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
            QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
            string mask = null, int? maxAllowedAnswers = null, bool? yesNoView = null, bool? areAnswersOrdered = null, bool hideIfDisabled = false,
            QuestionProperties properties = null, IList<ValidationCondition> validationConditions = null)
            {
                return new NewQuestionAdded(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: hideIfDisabled,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    properties: properties ?? new QuestionProperties(false, false),
                    responsibleId: responsibleId ?? Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: linkedToQuestionId,
                    areAnswersOrdered: areAnswersOrdered,
                    yesNoView: yesNoView,
                    maxAllowedAnswers: maxAllowedAnswers,
                    mask: mask,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    validationConditions: validationConditions ?? new List<ValidationCondition>());
            }*/

            public static NumericQuestionChanged NumericQuestionChanged(
                Guid publicKey,
                bool? isInteger = null,
                string stataExportCaption = null,
                string questionText = null,
                string variableLabel = null,
                bool featured = false,
                string conditionExpression = null,
                string validationExpression = null,
                string validationMessage = null,
                string instructions = null,
                Guid? responsibleId = null,
                bool hideIfDisabled = false,
            QuestionProperties properties = null)
            {
                return new NumericQuestionChanged(
                    publicKey: publicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: QuestionScope.Interviewer,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: hideIfDisabled,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    properties: properties ?? new QuestionProperties(false, false),
                    responsibleId: responsibleId ?? Guid.NewGuid(),
                    capital: false,
                    isInteger: isInteger,
                    countOfDecimalPlaces: null,
                    validationConditions: new List<ValidationCondition>());
            }

            public static NumericQuestionCloned NumericQuestionCloned(Guid publicKey,
                Guid sourceQuestionId,
                Guid groupPublicKey,
                bool? isInteger = null,
                string stataExportCaption = null,
                string questionText = null,
                string variableLabel = null,
                bool featured = false,
                string conditionExpression = null,
                string validationExpression = null,
                string validationMessage = null,
                string instructions = null,
                Guid? responsibleId = null,
                int targetIndex = 0,
                QuestionScope scope = QuestionScope.Interviewer,
                IList<ValidationCondition> validationConditions = null,
                bool hideIfDisabled = false,
            QuestionProperties properties = null)
            {
                return new NumericQuestionCloned(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: scope,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: hideIfDisabled,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    properties: properties ?? new QuestionProperties(false, false),
                    responsibleId: responsibleId ?? Guid.NewGuid(),
                    capital: false,
                    isInteger: isInteger,
                    countOfDecimalPlaces: null,
                    sourceQuestionnaireId: null,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex,
                    validationConditions: validationConditions ?? new List<ValidationCondition>());
            }

            public static QuestionChanged QuestionChanged(Guid questionId, string variableName, QuestionType questionType)
            {
                return QuestionChanged(
                    publicKey : questionId,
                    stataExportCaption : variableName,
                    questionType : questionType
                );
            }

            public static QuestionChanged QuestionChanged(Guid publicKey, Guid targetGroupKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
                bool hideIfDisabled = false,
            QuestionProperties properties = null, bool isTimestamp = false)
            {
                return new QuestionChanged(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: null,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: hideIfDisabled,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    properties: properties ?? new QuestionProperties(false, false),
                    responsibleId: responsibleId ?? Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: null,
                    linkedToRosterId: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    maxAllowedAnswers: null,
                    mask: null,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    targetGroupKey: targetGroupKey,
                    validationConditions: new List<ValidationCondition>(),
                linkedFilterExpression: null,
                isTimestamp: isTimestamp);
            }

            public static QuestionChanged QuestionChanged(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
            QuestionProperties properties = null, bool isTimestamp = false)
            {
                return new QuestionChanged(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: null,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: false,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    properties: properties ?? new QuestionProperties(false, false),
                    responsibleId: responsibleId ?? Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: null,
                    linkedToRosterId: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    maxAllowedAnswers: null,
                    mask: null,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    targetGroupKey: Guid.NewGuid(),
                    validationConditions: new List<ValidationCondition>(),
                linkedFilterExpression: null,
                isTimestamp: isTimestamp);
            }


            public static QuestionCloned QuestionCloned(Guid publicKey, Guid sourceQuestionId, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, string variableLabel = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
                Guid? sourceQuestionnaireId = null, int targetIndex = 0, int? maxAnswerCount = null, int? countOfDecimalPlaces = null,
                IList<ValidationCondition> validationConditions = null,
            QuestionProperties properties = null, bool isTimestamp = false)
            {
                return new QuestionCloned(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: false,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    properties: properties ?? new QuestionProperties(false, false),
                    responsibleId: responsibleId ?? Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: null, 
                    linkedToRosterId: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    maxAllowedAnswers: null,
                    mask: null,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    sourceQuestionnaireId: sourceQuestionnaireId,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex,
                    maxAnswerCount: maxAnswerCount,
                    countOfDecimalPlaces: countOfDecimalPlaces,
                    validationConditions: validationConditions ?? new List<ValidationCondition>(),
                linkedFilterExpression: null,
                isTimestamp: isTimestamp);
            }

            public static RosterChanged RosterChanged(Guid rosterId, RosterSizeSourceType rosterType, FixedRosterTitle[] titles)
            {
                return new RosterChanged(Guid.NewGuid(), rosterId)
                {
                    RosterSizeQuestionId = null,
                    RosterSizeSource = rosterType,
                    FixedRosterTitles = titles,
                    RosterTitleQuestionId = null
                };
            }
            
            public static NumericQuestionChanged UpdateNumericIntegerQuestion(Guid questionId, string variableName, string enablementCondition = null, string validationExpression = null)
            {
                return NumericQuestionChanged
                (
                    publicKey : questionId,
                    stataExportCaption : variableName,
                    isInteger : true,
                    conditionExpression : enablementCondition,
                    validationExpression : validationExpression
                );
            }
            
            public static VariableUpdated VariableUpdated(Guid? entityId = null, Guid? responsibleId = null, 
                VariableType variableType = VariableType.Boolean, string variableName = null, string variableExpression = null)
            {
                return new VariableUpdated(
                    entityId.GetValueOrDefault(Guid.NewGuid()),
                    responsibleId ?? Guid.NewGuid(),
                    new VariableData(
                        variableType,
                        variableName,
                        variableExpression
                        ));
            }
            public static VariableCloned VariableCloned(Guid? entityId = null, Guid? responsibleId = null, Guid? parentId = null,
                Guid? sourceQuestionnaireId = null, Guid? sourceEntityId = null, int targetIndex = 0,
                VariableType variableType = VariableType.Boolean, string variableName = null, string variableExpression = null)
            {
                return new VariableCloned(
                    entityId.GetValueOrDefault(Guid.NewGuid()),
                    responsibleId ?? Guid.NewGuid(),
                    parentId ?? Guid.NewGuid(),
                    sourceQuestionnaireId,
                    sourceEntityId ?? Guid.NewGuid(),
                    targetIndex,
                    new VariableData(
                        variableType,
                        variableName,
                        variableExpression
                        ));
            }
            
            public static GroupBecameARoster GroupBecameARosterEvent(string groupId)
            {
                return new GroupBecameARoster(responsibleId: new Guid(), groupId: Guid.Parse(groupId));
            }

            public static GroupCloned GroupClonedEvent(string groupId, string groupTitle = null,
                string parentGroupId = null)
            {
                return new GroupCloned()
                {
                    PublicKey = Guid.Parse(groupId),
                    ParentGroupPublicKey = GetQuestionnaireItemParentId(parentGroupId),
                    GroupText = groupTitle,
                    TargetIndex = 0
                };
            }

            
            public static GroupStoppedBeingARoster GroupStoppedBeingARosterEvent(string groupId)
            {
                return new GroupStoppedBeingARoster(responsibleId: new Guid(), groupId: Guid.Parse(groupId));
            }

            public static GroupUpdated GroupUpdatedEvent(string groupId, string groupTitle)
            {
                return new GroupUpdated()
                {
                    GroupPublicKey = Guid.Parse(groupId),
                    GroupText = groupTitle
                };
            }

            public static MultimediaQuestionUpdated MultimediaQuestionUpdatedEvent(string questionId, string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
            {
                return new MultimediaQuestionUpdated()
                {
                    QuestionId = Guid.Parse(questionId),
                    VariableName = questionVariable,
                    Title = questionTitle,
                    EnablementCondition = questionConditionExpression
                };
            }

            public static NewGroupAdded NewGroupAddedEvent(string groupId, string parentGroupId = null,
                string groupTitle = null,
                string enablementCondition = null)
            {
                return new NewGroupAdded()
                {
                    PublicKey = Guid.Parse(groupId),
                    ParentGroupPublicKey = GetQuestionnaireItemParentId(parentGroupId),
                    GroupText = groupTitle,
                    ConditionExpression = enablementCondition
                };
            }
            
            public static IPublishedEvent<NewQuestionnaireCreated> NewQuestionnaireCreatedEvent(string questionnaireId,
                string questionnaireTitle = null,
                bool? isPublic = null)
            {
                return new NewQuestionnaireCreated()
                {
                    PublicKey = new Guid(questionnaireId),
                    Title = questionnaireTitle,
                    IsPublic = isPublic ?? false
                }.ToPublishedEvent(new Guid(questionnaireId));
            }

            public static NumericQuestionChanged NumericQuestionChangedEvent(string questionId,
                string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
            {
                return Event.NumericQuestionChanged(
                    publicKey: Guid.Parse(questionId),
                    stataExportCaption: questionVariable,
                    questionText: questionTitle,
                    conditionExpression: questionConditionExpression
                    );
            }

            public static NumericQuestionCloned NumericQuestionClonedEvent(string questionId = null,
                string parentGroupId = null, string questionVariable = null, string questionTitle = null,
                string questionConditionExpression = null, string sourceQuestionId = null)
            {
                return Event.NumericQuestionCloned(
                    publicKey: GetQuestionnaireItemId(questionId),
                    groupPublicKey: GetQuestionnaireItemId(parentGroupId),
                    stataExportCaption: questionVariable,
                    questionText: questionTitle,
                    conditionExpression: questionConditionExpression,
                    sourceQuestionId: GetQuestionnaireItemId(sourceQuestionId),
                    targetIndex: 0
                    );
            }

            public static IPublishableEvent PublishableEvent(Guid? eventSourceId = null, IEvent payload = null)
            {
                return Mock.Of<IPublishableEvent>(_ => _.Payload == (payload ?? Mock.Of<IEvent>()) && _.EventSourceId == (eventSourceId ?? Guid.NewGuid()));
            }

            public static QRBarcodeQuestionCloned QRBarcodeQuestionClonedEvent(string questionId = null,
                string parentGroupId = null, string questionVariable = null, string questionTitle = null,
                string questionConditionExpression = null, string sourceQuestionId = null,
                IList<ValidationCondition> validationConditions = null)
            {
                return new QRBarcodeQuestionCloned()
                {
                    QuestionId = GetQuestionnaireItemId(questionId),
                    ParentGroupId = GetQuestionnaireItemId(parentGroupId),
                    VariableName = questionVariable,
                    Title = questionTitle,
                    EnablementCondition = questionConditionExpression,
                    SourceQuestionId = GetQuestionnaireItemId(sourceQuestionId),
                    TargetIndex = 0,
                    ValidationConditions = validationConditions ?? new List<ValidationCondition>()
                };
            }

            public static QuestionChanged QuestionChangedEvent(string questionId, string parentGroupId = null,
                string questionVariable = null, string questionTitle = null, QuestionType? questionType = null, string questionConditionExpression = null)
            {
                return Event.QuestionChanged(
                    publicKey: Guid.Parse(questionId),
                    groupPublicKey: Guid.Parse(parentGroupId ?? Guid.NewGuid().ToString()),
                    stataExportCaption: questionVariable,
                    questionText: questionTitle,
                    questionType: questionType ?? QuestionType.Text,
                    conditionExpression: questionConditionExpression
                    );
            }

            public static QuestionCloned QuestionClonedEvent(string questionId = null,
                string parentGroupId = null, string questionVariable = null, string questionTitle = null,
                QuestionType questionType = QuestionType.Text, string questionConditionExpression = null,
                string sourceQuestionId = null,
                IList<ValidationCondition> validationConditions = null, bool hideIfDisabled = false)
            {
                return new QuestionCloned(
                    publicKey: GetQuestionnaireItemId(questionId),
                    groupPublicKey: GetQuestionnaireItemId(parentGroupId),
                    stataExportCaption: questionVariable,
                    questionText: questionTitle,
                    questionType: questionType,
                    conditionExpression: questionConditionExpression,
                    hideIfDisabled: hideIfDisabled,
                    sourceQuestionId: GetQuestionnaireItemId(sourceQuestionId),
                    targetIndex: 0,
                    featured: false,
                    instructions: null,
                    properties: Create.QuestionProperties(),
                    responsibleId: Guid.NewGuid(),
                    capital: false,
                    questionScope: QuestionScope.Interviewer,
                    variableLabel: null,
                    validationExpression: null,
                    validationMessage: null,
                    answerOrder: null,
                    answers: null,
                    linkedToQuestionId: null,
                    linkedToRosterId: null,
                    isInteger: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    mask: null,
                    maxAllowedAnswers: null,
                    isFilteredCombobox: null,
                    cascadeFromQuestionId: null,
                    sourceQuestionnaireId: null,
                    maxAnswerCount: null,
                    countOfDecimalPlaces: null,
                    validationConditions: validationConditions,
                    linkedFilterExpression: null,
                    isTimestamp: false
                    );
            }
            
            public static QuestionnaireItemMoved QuestionnaireItemMovedEvent(string itemId,
                string targetGroupId = null, int? targetIndex = null, string questionnaireId = null)
            {
                return new QuestionnaireItemMoved()
                {
                    PublicKey = Guid.Parse(itemId),
                    GroupKey = GetQuestionnaireItemParentId(targetGroupId),
                    TargetIndex = targetIndex ?? 0
                };
            }

            public static QuestionnaireUpdated QuestionnaireUpdatedEvent(string questionnaireId,
                string questionnaireTitle,
                bool isPublic = false)
            {
                return new QuestionnaireUpdated() { Title = questionnaireTitle, IsPublic = isPublic };
            }

            public static RosterChanged RosterChanged(string groupId)
            {
                return new RosterChanged(responsibleId: new Guid(), groupId: Guid.Parse(groupId));
            }

            public static SharedPersonFromQuestionnaireRemoved SharedPersonFromQuestionnaireRemoved(Guid questionnaireId, Guid personId)
            {
                return new SharedPersonFromQuestionnaireRemoved() { PersonId = personId };
            }

            public static SharedPersonToQuestionnaireAdded SharedPersonToQuestionnaireAdded(Guid questionnaireId, Guid personId)
            {
                return new SharedPersonToQuestionnaireAdded() { PersonId = personId };
            }
           
            public static TemplateImported TemplateImportedEvent(
                string questionnaireId,
                string chapter1Id = null,
                string chapter1Title = null,
                string chapter2Id = null,
                string chapter2Title = null,
                string questionnaireTitle = null,
                string chapter1GroupId = null, string chapter1GroupTitle = null,
                string chapter2QuestionId = null,
                string chapter2QuestionTitle = null,
                string chapter2QuestionVariable = null,
                string chapter2QuestionConditionExpression = null,
                string chapter1StaticTextId = null, string chapter1StaticText = null,
                bool? isPublic = null)
            {
                return new TemplateImported()
                {
                    Source = CreateQuestionnaireDocument(questionnaireId: questionnaireId, questionnaireTitle: questionnaireTitle,
                        chapter1Id: chapter1Id ?? Guid.NewGuid().FormatGuid(), chapter1Title: chapter1Title,
                        chapter2Id: chapter2Id ?? Guid.NewGuid().FormatGuid(),
                        chapter2Title: chapter2Title, chapter1GroupId: chapter1GroupId,
                        chapter1GroupTitle: chapter1GroupTitle, chapter2QuestionId: chapter2QuestionId,
                        chapter2QuestionTitle: chapter2QuestionTitle, chapter2QuestionVariable: chapter2QuestionVariable,
                        chapter2QuestionConditionExpression: chapter2QuestionConditionExpression,
                        chapter1StaticTextId: chapter1StaticTextId, chapter1StaticText: chapter1StaticText,
                        isPublic: isPublic ?? false)
                };
            }

            public static TextListQuestionChanged TextListQuestionChangedEvent(string questionId,
                string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
            {
                return new TextListQuestionChanged()
                {
                    PublicKey = Guid.Parse(questionId),
                    StataExportCaption = questionVariable,
                    QuestionText = questionTitle,
                    ConditionExpression = questionConditionExpression
                };
            }

            public static TextListQuestionCloned TextListQuestionClonedEvent(string questionId = null,
                string parentGroupId = null, string questionVariable = null, string questionTitle = null,
                string questionConditionExpression = null, string sourceQuestionId = null)
            {
                return new TextListQuestionCloned()
                {
                    PublicKey = GetQuestionnaireItemId(questionId),
                    GroupId = GetQuestionnaireItemId(parentGroupId),
                    StataExportCaption = questionVariable,
                    QuestionText = questionTitle,
                    ConditionExpression = questionConditionExpression,
                    SourceQuestionId = GetQuestionnaireItemId(sourceQuestionId),
                    TargetIndex = 0
                };
            }

            public static VariableCloned VariableClonedEvent(string entityId = null,
                string parentId = null, string sourceEntityId = null, string variableName = null, int targetIndex = 0)
            {
                return Create.Event.VariableCloned(
                    entityId: GetQuestionnaireItemId(entityId),
                    parentId: GetQuestionnaireItemId(parentId),
                    sourceEntityId: GetQuestionnaireItemId(sourceEntityId),
                    variableName: variableName,
                    targetIndex: targetIndex);
            }
        }

        public static IPublishedEvent<T> ToPublishedEvent<T>(this T @event,
                Guid? eventSourceId = null,
                string origin = null,
                DateTime? eventTimeStamp = null,
                Guid? eventId = null)
                where T : class, IEvent
        {
            var mock = new Mock<IPublishedEvent<T>>();
            var eventIdentifier = eventId ?? Guid.NewGuid();
            mock.Setup(x => x.Payload).Returns(@event);
            mock.Setup(x => x.EventSourceId).Returns(eventSourceId ?? Guid.NewGuid());
            mock.Setup(x => x.Origin).Returns(origin);
            mock.Setup(x => x.EventIdentifier).Returns(eventIdentifier);
            mock.Setup(x => x.EventTimeStamp).Returns((eventTimeStamp ?? DateTime.Now));
            var publishableEventMock = mock.As<IUncommittedEvent>();
            publishableEventMock.Setup(x => x.Payload).Returns(@event);
            return mock.Object;
        }
    }
}