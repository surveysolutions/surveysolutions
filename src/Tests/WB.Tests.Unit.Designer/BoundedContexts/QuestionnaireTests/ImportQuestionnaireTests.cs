extern alias designer;
using System;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class ImportQuestionnaireTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }


        [Test]
        public void Execute_When_SourceIsNotQuestionnaireDocument_Then_ArgumentException_should_be_thrown()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
                Mock<IQuestionnaireDocument> docMock = new Mock<IQuestionnaireDocument>();
                // act
                TestDelegate act =
                    () =>
                    questionnaire.ImportQuestionnaire(Guid.NewGuid(), docMock.Object);
                // assert
                Assert.Throws<QuestionnaireException>(act);
            }
        }

        [Test]
        public void Execute_When_QuestionnaireDocument_is_deleted_should_throw_an_exception()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid responsibleId = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
                var document = Create.QuestionnaireDocument();
                document.IsDeleted = true;

                // act
                TestDelegate act = () => questionnaire.ImportQuestionnaire(Guid.NewGuid(), document);

                // assert
                Assert.Throws<QuestionnaireException>(act);
            }
        }

    }
}
