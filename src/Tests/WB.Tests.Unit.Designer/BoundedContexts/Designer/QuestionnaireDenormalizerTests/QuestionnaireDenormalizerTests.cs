using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;

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
                Mock.Of<ILogger>(),
                Mock.Of<IClock>(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IAttachmentService>(),
                Mock.Of<ITranslationsService>(),
                Mock.Of<IQuestionnaireHistoryVersionsService>());
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
    }
}
