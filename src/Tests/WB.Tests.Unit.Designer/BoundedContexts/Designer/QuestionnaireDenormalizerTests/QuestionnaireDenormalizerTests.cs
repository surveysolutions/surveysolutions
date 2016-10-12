using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    [TestFixture]
    internal class QuestionnaireDenormalizerTests
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void HandleQuestionChanged_When_QuestionUpdate_event_is_come_Then_all_abstractQuestion_fields_are_updated()
        {
            // Arrange
            var questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var questionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var personId = Guid.Parse("22222222-2222-2222-2222-222222222221");

            var innerDocument = CreateQuestionnaireDocument(questionnaireId, personId);

            innerDocument
                .AddChapter(Guid.NewGuid())
                .AddQuestion(questionId);

            var denormalizer = CreateQuestionnaireDenormalizer(innerDocument);

            var command = new UpdateTextQuestion(denormalizer.Id, questionId, personId, new CommonQuestionParameters ()
                {
                    Title = "What is your name",
                    Instructions = "Instructuis"
                },
                null,QuestionScope.Interviewer, 
                false,
                new List<ValidationCondition> { new ValidationCondition("[this]!=''", "Empty names is invalid answer") });

            // Act
            denormalizer.UpdateTextQuestion(command);

            // Assert
            #warning: updated question is a new entity, that's why we should search for it by it's id
            var question = innerDocument.Find<IQuestion>(questionId);

            Assert.That(command.Title, Is.EqualTo(question.QuestionText));
            Assert.That(QuestionType.Text, Is.EqualTo(question.QuestionType));
            Assert.That(command.IsPreFilled, Is.EqualTo(question.Featured));
            Assert.That(command.EnablementCondition, Is.EqualTo(question.ConditionExpression));
            Assert.That(command.Instructions, Is.EqualTo(question.Instructions));
            Assert.That(command.VariableName, Is.EqualTo(question.StataExportCaption));
            Assert.That(command.ValidationConditions, Is.EqualTo(question.ValidationConditions));
        }

        public static Questionnaire CreateQuestionnaireDenormalizer(QuestionnaireDocument document,
            IExpressionProcessor expressionProcessor = null)
        {
            var questionnaireAr = new Questionnaire(
                new QuestionnaireEntityFactory(),
                Mock.Of<ILogger>(),
                Mock.Of<IClock>(),
                expressionProcessor ?? Mock.Of<IExpressionProcessor>(),
                Create.SubstitutionService(),
                Create.KeywordsProvider(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IAttachmentService>(),
                Mock.Of<ITranslationsService>());
            questionnaireAr.Initialize(document.PublicKey, document, Enumerable.Empty<SharedPerson>());
            return questionnaireAr;
        }

        private static QuestionnaireDocument CreateQuestionnaireDocument(Guid questionnaireId, Guid? createdBy = null)
        {
            var innerDocument = new QuestionnaireDocument
            {
                Title = string.Format("Questionnaire {0}", questionnaireId),
                PublicKey = questionnaireId,
                CreatedBy = createdBy
            };
            return innerDocument;
        }

        private static QuestionChanged CreateQuestionChangedEvent(Guid questionId, QuestionType type = QuestionType.Text)
        {
            return CreateQuestionChanged(
                    questionText : "What is your name",
                    questionType : type,
                    publicKey : questionId,
                    featured : true,
                    answerOrder : Order.AsIs,
                    conditionExpression : string.Empty,
                    answers : null,
                    instructions : "Answer this question, please",
                    stataExportCaption : "name",
                    validationConditions: new List<ValidationCondition> { new ValidationCondition("[this]!=''", "Empty names is invalid answer") }
                );
        }

        private static IPublishedEvent<T> CreatePublishedEvent<T>(Guid questionnaireId, T evnt)
            where T : IEvent
        {
            IPublishedEvent<T> e = new PublishedEvent<T>(Create.Event.PublishableEvent(eventSourceId:questionnaireId, payload: evnt));
            return e;
        }

        public static QuestionChanged CreateQuestionChanged(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
            string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
            QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
            QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null,
            Guid? cascadeFromQuestionId = null, string conditionExpression = null, bool hideIfDisabled = false, Order? answerOrder = null,
            IList<ValidationCondition> validationConditions = null, bool isTimestamp = false)
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
                properties: Create.QuestionProperties(),
                responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
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
                    validationConditions: validationConditions ?? new List<ValidationCondition>(),
                linkedFilterExpression: null,
                isTimestamp: isTimestamp);
        }
    }
}