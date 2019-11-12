using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;


namespace WB.Tests.Integration.WebTester.Services
{
    public class when_interview_exception_occures : AppdomainsPerInterviewManagerTestsBase
    {
        private Guid interviewId = Guid.NewGuid();
        private Guid numericQuestionId = Id.gB;
        private Guid interviewerId = Id.g1;

        [SetUp]
        public void Setup()
        {
            Manager = CreateManager();
            var numericIntegerQuestion = Create.Entity.NumericIntegerQuestion(id: numericQuestionId);
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(numericIntegerQuestion);
            SetupAppDomainInterview(Manager, interviewId, questionnaire);
        }

        [Test]
        public void should_return_exception_to_caller()
        {
            var exception = Assert.Throws<AnswerNotAcceptedException>(() =>
                Manager.Execute(Create.Command.AnswerTextQuestionCommand(interviewId, interviewId, questionId: numericQuestionId, answer: "answer")));

            Assert.That(exception.ExceptionType, Is.EqualTo(InterviewDomainExceptionType.AnswerNotAccepted));
        }

        [TearDown]
        public void TearDown()
        {
            Manager.TearDown(interviewId);
        }
    }
}
