using System;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    public class ImportQuestionnaireTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void CreateNewSnapshot_When_ArgumentIsNotNull_Then_TemplateImportedEventIsRised()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                var newState = new QuestionnaireDocument();

                // act
                questionnaire.ImportQuestionnaire(Guid.NewGuid(),newState);

                // assert
                Assert.That(GetSingleEvent<TemplateImported>(eventContext).Source, Is.EqualTo(newState));
            }
        }


        [Test]
        public void Execute_When_SourceIsNotQuestionnaireDocument_Then_ArgumentException_should_be_thrown()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                Mock<IQuestionnaireDocument> docMock = new Mock<IQuestionnaireDocument>();
                // act
                TestDelegate act =
                    () =>
                    questionnaire.ImportQuestionnaire(Guid.NewGuid(), docMock.Object);
                // assert
                Assert.Throws<DomainException>(act);
            }
        }
    }
}
