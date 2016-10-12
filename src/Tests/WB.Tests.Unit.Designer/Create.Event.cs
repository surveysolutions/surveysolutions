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
           
            public static GroupUpdated GroupUpdatedEvent(string groupId, string groupTitle)
            {
                return new GroupUpdated()
                {
                    GroupPublicKey = Guid.Parse(groupId),
                    GroupText = groupTitle
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