using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Events.Questionnaire;
using Moq;
using NUnit.Framework;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    public class ImportQuestionnaireTests
    {

        [Test]
        public void CreateNewSnapshot_When_ArgumentIsNotNull_Then_TemplateImportedEventIsRised()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();
                var newState = new QuestionnaireDocument();

                // act
                questionnaire.ImportQuestionnaire(Guid.NewGuid(),newState);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<TemplateImported>(eventContext).Source, Is.EqualTo(newState));
            }
        }


        [Test]
        public void Execute_When_SourceIsNotQuestionnaireDocument_Then_ArgumentException_should_be_thrown()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireAR();
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
