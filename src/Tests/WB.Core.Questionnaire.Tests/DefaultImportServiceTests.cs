using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Main.Core.View;
using Moq;
using NUnit.Framework;
using Ncqrs.Spec;
using WB.Core.Questionnaire.ImportService;
using WB.Core.Questionnaire.ImportService.Commands;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.Core.Questionnaire.Tests
{
    [TestFixture]
    public class DefaultImportServiceTests
    {
        [Test]
        public void Execute_When_UserExistsSourceIsDocument_Then_NewQuestionnaireIsSaved()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                DefaultImportService importService = CreateDefaultImportService();
                var questionnaireId = Guid.NewGuid();
                var creatorId = Guid.NewGuid();
                // act
                importService.Execute(new ImportQuestionnaireCommand(creatorId, new QuestionnaireDocument() { Title = "new questionnaire", PublicKey = questionnaireId}));
                // assert
                var risedEvent = GetSingleEvent<NewQuestionnaireCreated>(eventContext);
                Assert.AreEqual(questionnaireId,risedEvent.PublicKey);
                Assert.AreEqual(creatorId,risedEvent.CreatedBy);
                Assert.AreEqual("new questionnaire", risedEvent.Title);
            }
        }

        /*[Test]
        public void Execute_When_UserIsAbsent_Then_ArgumentException_should_be_thrown()
        {
            // arrange
            DefaultImportService importService = CreateDefaultImportService();

            // act
            TestDelegate act =
                () =>
                importService.Execute(new ImportQuestionnaireCommand(Guid.NewGuid(), new QuestionnaireDocument()));
            Assert.Throws<ArgumentException>(act);
            // assert
            
        }*/

      /*  [Test]
        public void Execute_When_QuestionnairePresentWithSameId_Then_ArgumentException_should_be_thrown()
        {
            // arrange
            DefaultImportService importService = CreateDefaultImportService();
            var questionnaireId = Guid.NewGuid();
            this.viewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(im => im.QuestionnaireId == questionnaireId)))
                .Returns(new QuestionnaireView(new QuestionnaireDocument()));
            // act
            TestDelegate act =
                () =>
                importService.Execute(new ImportQuestionnaireCommand(Guid.NewGuid(),
                                                                     new QuestionnaireDocument(){ PublicKey = questionnaireId}));
            Assert.Throws<ArgumentException>(act);
            // assert

        }*/

        [Test]
        public void Execute_When_SourceIsNotQuestionnaireDocument_Then_ArgumentException_should_be_thrown()
        {
            // arrange
            DefaultImportService importService = CreateDefaultImportService();
            Mock<IQuestionnaireDocument> docMock=new Mock<IQuestionnaireDocument>();
            // act
            TestDelegate act =
                () =>
                importService.Execute(new ImportQuestionnaireCommand( Guid.NewGuid(), docMock.Object));
            Assert.Throws<ArgumentException>(act);
            // assert
            
        }

        private DefaultImportService CreateDefaultImportService()
        {
            return new DefaultImportService();
        }
        private static T GetSingleEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Single(e => e.Payload is T).Payload;
        }

    }
}
