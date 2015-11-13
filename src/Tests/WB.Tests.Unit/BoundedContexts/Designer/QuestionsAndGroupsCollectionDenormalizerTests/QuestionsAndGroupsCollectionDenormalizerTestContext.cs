extern alias designer;

using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using TemplateImported = designer::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionsAndGroupsCollectionDenormalizerTests
{
    [Subject(typeof(QuestionsAndGroupsCollectionDenormalizer))]
    internal class QuestionsAndGroupsCollectionDenormalizerTestContext
    {
        protected static QuestionsAndGroupsCollectionDenormalizer CreateQuestionnaireInfoDenormalizer(
            IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView> readsideRepositoryWriter = null,
            IQuestionDetailsViewMapper questionDetailsViewMapper = null, 
            IQuestionnaireEntityFactory questionnaireEntityFactory = null)
        {

            return new QuestionsAndGroupsCollectionDenormalizer(readsideRepositoryWriter ?? Mock.Of<IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView>>(),
                questionDetailsViewMapper ?? Mock.Of<IQuestionDetailsViewMapper>(),
                questionnaireEntityFactory ?? Mock.Of<IQuestionnaireEntityFactory>());
        }

        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event)
           where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event);
        }

        protected static IPublishedEvent<GroupDeleted> CreateGroupDeletedEvent(Guid groupId)
        {
            return ToPublishedEvent(new GroupDeleted
            {
                GroupPublicKey = groupId,
            });
        }

        protected static IPublishedEvent<QuestionDeleted> CreateQuestionDeletedEvent(Guid questionId)
        {
            return ToPublishedEvent(new QuestionDeleted
            {
                QuestionId = questionId,
            });
        }

        protected static IPublishedEvent<NewGroupAdded> CreateNewGroupAddedEvent(Guid groupId, Guid parentGroupId, string enablementCondition, string description, string title)
        {
            return ToPublishedEvent(new NewGroupAdded
            {
                PublicKey = groupId,
                GroupText = title,
                ParentGroupPublicKey = parentGroupId,
                Description = description,
                ConditionExpression = enablementCondition
            });
        }

        protected static IPublishedEvent<GroupCloned> CreateGroupClonedEvent(Guid groupId, Guid parentGroupId, string enablementCondition, string description, string title)
        {
            return ToPublishedEvent(new GroupCloned
            {
                PublicKey = groupId,
                GroupText = title,
                ParentGroupPublicKey = parentGroupId,
                Description = description,
                ConditionExpression = enablementCondition
            });
        }

        protected static IPublishedEvent<GroupUpdated> CreateGroupUpdatedEvent(Guid groupId,
            string title = "Updated Group Title X")
        {
            return ToPublishedEvent(new GroupUpdated
            {
                GroupPublicKey = groupId,
                GroupText = title,
            });
        }

        protected static IPublishedEvent<NewQuestionAdded> CreateNewQuestionAddedEvent(Guid questionId, Guid? groupId = null, string title = "New Question X")
        {
            return ToPublishedEvent(CreateNewQuestionAdded
            (
                publicKey : questionId,
                groupPublicKey : groupId,
                questionText : title,
                questionType : QuestionType.Numeric
            ));
        }

        protected static IPublishedEvent<QuestionChanged> CreateQuestionChangedEvent(Guid questionId, string title, QuestionType questionType = QuestionType.Numeric)
        {
            return ToPublishedEvent(CreateQuestionChanged
            (
                publicKey : questionId,
                questionType : questionType,
                questionText : title
            ));
        }

        protected static IPublishedEvent<QuestionnaireItemMoved> CreateQuestionnaireItemMovedEvent(Guid itemId, Guid? targetGroupId)
        {
            return ToPublishedEvent(new QuestionnaireItemMoved
            {
                PublicKey = itemId,
                GroupKey = targetGroupId,
            });
        }

        protected static IPublishedEvent<GroupBecameARoster> CreateGroupBecameARosterEvent(Guid groupId)
        {
            return ToPublishedEvent(new GroupBecameARoster(Guid.NewGuid(), groupId));
        }

        protected static IPublishedEvent<GroupStoppedBeingARoster> CreateGroupStoppedBeingARosterEvent(Guid groupId)
        {
            return ToPublishedEvent(new GroupStoppedBeingARoster(Guid.NewGuid(), groupId));
        }

        protected static IPublishedEvent<RosterChanged> CreateRosterChangedEvent(Guid groupId, Guid? rosterSizeQuestionId, 
            RosterSizeSourceType rosterSizeSource, FixedRosterTitle[] rosterFixedTitles, Guid? rosterTitleQuestionId)
        {
            return
                ToPublishedEvent(new RosterChanged(Guid.NewGuid(), groupId){
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = rosterSizeSource,
                    FixedRosterTitles =  rosterFixedTitles,
                    RosterTitleQuestionId = rosterTitleQuestionId 
                });
        }

        protected static IPublishedEvent<NumericQuestionAdded> CreateNumericQuestionAddedEvent(
            Guid questionId, Guid? parentGroupId = null)
        {
            return ToPublishedEvent(CreateNumericQuestionAdded
            (
                publicKey : questionId,
                groupPublicKey : parentGroupId ?? Guid.NewGuid()
            ));
        }

        protected static IPublishedEvent<QRBarcodeQuestionAdded> CreateQRBarcodeQuestionAddedEvent(
            Guid questionId, Guid parentGroupId)
        {
            return ToPublishedEvent(new QRBarcodeQuestionAdded
            {
                QuestionId = questionId,
                ParentGroupId = parentGroupId
            });
        }

        protected static IPublishedEvent<StaticTextAdded> CreateStaticTextAddedEvent(Guid entityId, Guid parentId, string text = null)
        {
            return ToPublishedEvent(new StaticTextAdded
            {
                EntityId = entityId,
                ParentId = parentId,
                Text = text
            });
        }

        protected static IPublishedEvent<TextListQuestionAdded> CreateTextListQuestionAddedEvent(
            Guid questionId, Guid parentGroupId)
        {
            return ToPublishedEvent(new TextListQuestionAdded
            {
                PublicKey = questionId,
                GroupId = parentGroupId
            });
        }

        protected static IPublishedEvent<TextListQuestionCloned> CreateTextListQuestionClonedEvent(Guid questionId, Guid? groupId = null, Guid? sourceQuestionId = null)
        {
            return ToPublishedEvent(new TextListQuestionCloned
            {
                PublicKey = questionId,
                SourceQuestionId = sourceQuestionId ?? Guid.NewGuid(),
                GroupId = groupId ?? Guid.NewGuid(),
            });
        }

        protected static IPublishedEvent<TextListQuestionChanged> CreateTextListQuestionChangedEvent(Guid questionId)
        {
            return ToPublishedEvent(new TextListQuestionChanged
            {
                PublicKey = questionId
            });
        }

        protected static IPublishedEvent<QRBarcodeQuestionUpdated> CreateQRBarcodeQuestionUpdatedEvent(
            Guid questionId)
        {
            return ToPublishedEvent(new QRBarcodeQuestionUpdated
            {
                QuestionId = questionId,
            });
        }

        protected static IPublishedEvent<MultimediaQuestionUpdated> CreateMultimediaQuestionUpdatedEvent(Guid questionId)
        {
            return ToPublishedEvent(new MultimediaQuestionUpdated()
            {
                QuestionId = questionId,
            });
        }

        protected static IPublishedEvent<StaticTextUpdated> CreateStaticTextUpdatedEvent(Guid entityId, string text = null)
        {
            return ToPublishedEvent(new StaticTextUpdated()
            {
                EntityId = entityId,
                Text = text
            });
        }

        protected static IPublishedEvent<NumericQuestionChanged> CreateNumericQuestionChangedEvent(
            Guid questionId)
        {
            return ToPublishedEvent(CreateNumericQuestionChanged
            (
                publicKey : questionId
            ));
        }

        protected static IPublishedEvent<StaticTextCloned> CreateStaticTextClonedEvent(Guid entityId, Guid parentId,
            Guid? sourceEntityId = null, string text = null)
        {
            return ToPublishedEvent(new StaticTextCloned
            {
                EntityId = entityId,
                SourceEntityId = sourceEntityId ?? Guid.NewGuid(),
                ParentId = parentId,
                Text = text
            });
        }

        protected static IPublishedEvent<StaticTextDeleted> CreateStaticTextDeletedEvent(Guid entityId)
        {
            return ToPublishedEvent(new StaticTextDeleted()
            {
                EntityId = entityId
            });
        }

        protected static IPublishedEvent<QRBarcodeQuestionCloned> CreateQRBarcodeQuestionClonedEvent(Guid questionId,Guid parentGroupId, Guid? sourceQuestionId = null)
        {
            return ToPublishedEvent(new QRBarcodeQuestionCloned
            {
                QuestionId = questionId,
                SourceQuestionId = sourceQuestionId ?? Guid.NewGuid(),
                ParentGroupId = parentGroupId
            });
        }
        protected static IPublishedEvent<NumericQuestionCloned> CreateNumericQuestionClonedEvent(
            Guid questionId, Guid? sourceQuestionId = null, Guid? parentGroupId = null)
        {
            return ToPublishedEvent(CreateNumericQuestionCloned
            (
                publicKey : questionId,
                sourceQuestionId : sourceQuestionId ?? Guid.NewGuid(),
                groupPublicKey : parentGroupId ?? Guid.NewGuid()
            ));
        }

        protected static IPublishedEvent<QuestionCloned> CreateQuestionClonedEvent(
            Guid questionId, QuestionType questionType = QuestionType.Numeric, Guid? sourceQuestionId = null, Guid? parentGroupId = null)
        {
            return ToPublishedEvent(CreateQuestionCloned
            (
                publicKey : questionId,
                questionType : questionType,
                sourceQuestionId : sourceQuestionId ?? Guid.NewGuid(),
                groupPublicKey : parentGroupId ?? Guid.NewGuid()
            ));
        }

        protected static IPublishedEvent<QuestionnaireDeleted> CreateQuestionnaireDeletedEvent()
        {
            return ToPublishedEvent(new QuestionnaireDeleted
            {
            });
        }

        protected static IPublishedEvent<NewQuestionnaireCreated> CreateNewQuestionnaireCreatedEvent()
        {
            return ToPublishedEvent(new NewQuestionnaireCreated
            {
            });
        }


        protected static IPublishedEvent<TemplateImported> CreateTemplateImportedEvent(QuestionnaireDocument questionnaireDocument = null)
        {
            return ToPublishedEvent(new TemplateImported
            {
                Source = questionnaireDocument ?? new QuestionnaireDocument()
            });
        }

        protected static IPublishedEvent<QuestionnaireCloned> CreateQuestionnaireClonedEvent(QuestionnaireDocument questionnaireDocument = null)
        {
            return ToPublishedEvent(new QuestionnaireCloned
            {
                QuestionnaireDocument = questionnaireDocument ?? new QuestionnaireDocument()
            });
        }


        protected static IPublishedEvent<TextListQuestionAdded> CreateTextListQuestionAddedEvent(
            Guid questionId, Guid? parentGroupId = null, int? maxAnswerCount = null)
        {
            return ToPublishedEvent(new TextListQuestionAdded
            {
                PublicKey = questionId,
                GroupId = parentGroupId ?? Guid.NewGuid(),
                MaxAnswerCount = maxAnswerCount
            });
        }

        protected static IPublishedEvent<TextListQuestionCloned> TextListQuestionClonedEvent(
            Guid questionId, Guid? sourceQuestionId = null, Guid? parentGroupId = null, int? maxAnswerCount = null)
        {
            return ToPublishedEvent(new TextListQuestionCloned
            {
                PublicKey = questionId,
                SourceQuestionId = sourceQuestionId ?? Guid.NewGuid(),
                GroupId = parentGroupId ?? Guid.NewGuid(),
                MaxAnswerCount = maxAnswerCount
            });
        }

        protected static IPublishedEvent<TextListQuestionChanged> CreateTextListQuestionChangedEvent(
            Guid questionId, int? maxAnswerCount = null)
        {
            return ToPublishedEvent(new TextListQuestionChanged
            {
                PublicKey = questionId,
                MaxAnswerCount = maxAnswerCount

            });
        }

        protected static NewQuestionAdded CreateNewQuestionAdded(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
            string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
            QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
            QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null)
        {
            return new NewQuestionAdded(
                publicKey: publicKey,
                groupPublicKey: groupPublicKey,
                questionText: questionText,
                stataExportCaption: stataExportCaption,
                variableLabel: null,
                featured: featured,
                questionScope: questionScope,
                conditionExpression: conditionExpression,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                capital: capital,
                isInteger: isInteger,
                questionType: questionType,
                answerOrder: answerOrder,
                answers: answers,
                linkedToQuestionId: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                mask: null,
                isFilteredCombobox: isFilteredCombobox,
                cascadeFromQuestionId: cascadeFromQuestionId);
        }

        protected static QuestionCloned CreateQuestionCloned(Guid publicKey, Guid sourceQuestionId, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
            string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
            QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
            QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
            Guid? sourceQuestionnaireId = null, int targetIndex = 0, int? maxAnswerCount = null, int? countOfDecimalPlaces = null)
        {
            return new QuestionCloned(
                publicKey: publicKey,
                groupPublicKey: groupPublicKey,
                questionText: questionText,
                stataExportCaption: stataExportCaption,
                variableLabel: null,
                featured: featured,
                questionScope: questionScope,
                conditionExpression: conditionExpression,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                capital: capital,
                isInteger: isInteger,
                questionType: questionType,
                answerOrder: answerOrder,
                answers: answers,
                linkedToQuestionId: null,
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
                countOfDecimalPlaces: countOfDecimalPlaces);
        }
        protected static QuestionChanged CreateQuestionChanged(Guid publicKey, Guid targetGroupKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
            string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
            QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
            QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null)
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
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                capital: capital,
                isInteger: isInteger,
                questionType: questionType,
                answerOrder: answerOrder,
                answers: answers,
                linkedToQuestionId: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                mask: null,
                isFilteredCombobox: isFilteredCombobox,
                cascadeFromQuestionId: cascadeFromQuestionId,
                targetGroupKey: targetGroupKey);
        }

        protected static NumericQuestionAdded CreateNumericQuestionAdded(Guid publicKey, Guid groupPublicKey,
            bool? isInteger = null,
            string stataExportCaption = null,
            string questionText = null,
            string variableLabel = null,
            bool featured = false,
            string conditionExpression = null,
            string validationExpression = null,
            string validationMessage = null,
            string instructions = null,
            Guid? responsibleId = null)
        {
            return new NumericQuestionAdded(
                publicKey: publicKey,
                groupPublicKey: groupPublicKey,
                questionText: questionText,
                stataExportCaption: stataExportCaption,
                variableLabel: variableLabel,
                featured: featured,
                questionScope: QuestionScope.Interviewer,
                conditionExpression: conditionExpression,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                capital: false,
                isInteger: isInteger,
                countOfDecimalPlaces: null);
        }

        protected static NumericQuestionCloned CreateNumericQuestionCloned(Guid publicKey, 
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
            Guid? responsibleId = null)
        {
            return new NumericQuestionCloned(
                publicKey: publicKey,
                groupPublicKey: groupPublicKey,
                questionText: questionText,
                stataExportCaption: stataExportCaption,
                variableLabel: variableLabel,
                featured: featured,
                questionScope: QuestionScope.Interviewer,
                conditionExpression: conditionExpression,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                capital: false,
                isInteger: isInteger,
                countOfDecimalPlaces: null,
                sourceQuestionnaireId: null,
                sourceQuestionId: sourceQuestionId,
                targetIndex:0);
        }
        protected static NumericQuestionChanged CreateNumericQuestionChanged(
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
            Guid? responsibleId = null)
        {
            return new NumericQuestionChanged(
                publicKey: publicKey,
                questionText: questionText,
                stataExportCaption: stataExportCaption,
                variableLabel: variableLabel,
                featured: featured,
                questionScope: QuestionScope.Interviewer,
                conditionExpression: conditionExpression,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                capital: false,
                isInteger: isInteger,
                countOfDecimalPlaces: null);
        }

        protected static QuestionChanged CreateQuestionChanged(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
            string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
            QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
            QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null)
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
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                capital: capital,
                isInteger: isInteger,
                questionType: questionType,
                answerOrder: answerOrder,
                answers: answers,
                linkedToQuestionId: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                mask: null,
                isFilteredCombobox: isFilteredCombobox,
                cascadeFromQuestionId: cascadeFromQuestionId,
                targetGroupKey: Guid.NewGuid() );
        }
    }
}
