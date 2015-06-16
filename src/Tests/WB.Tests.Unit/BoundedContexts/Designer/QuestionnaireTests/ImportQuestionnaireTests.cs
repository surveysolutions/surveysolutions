extern alias designer;

using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using TemplateImported = designer::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    public class ImportQuestionnaireTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void CreateNewSnapshot_When_ArgumentIsNotNull_Then_TemplateImportedEventIsRised()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid responsibleId = Guid.NewGuid();
                Guid sharedPersonWith = Guid.NewGuid();
                Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
                questionnaire.Apply(new SharedPersonToQuestionnaireAdded() { PersonId = sharedPersonWith });
                var newState = new QuestionnaireDocument();
                // act
                questionnaire.ImportQuestionnaire(responsibleId,newState);

                // assert
                Assert.That(GetSingleEvent<TemplateImported>(eventContext).Source, Is.EqualTo(newState));
                Assert.That(questionnaire.CreateSnapshot().QuestionnaireDocument.SharedPersons.Contains(sharedPersonWith), Is.True);
            }
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
    }
}
