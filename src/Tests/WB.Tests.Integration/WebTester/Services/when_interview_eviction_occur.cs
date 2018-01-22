using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Integration.WebTester.Services
{
    public class when_interview_eviction_occur : AppdomainsPerInterviewManagerTestsBase
    {
        private Guid interviewId = Guid.NewGuid();
        private Guid numericQuestionId = Id.gB;
        private Guid interviewerId = Id.g1;
        private TokenEviction evictionNotification = new TokenEviction();

        [SetUp]
        public void Setup()
        {
            Manager = CreateManager(evictionNotification);

            var numericIntegerQuestion = Create.Entity.NumericIntegerQuestion(id: numericQuestionId);
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(numericIntegerQuestion);
            SetupAppDomainInterview(Manager, interviewId, questionnaire);
        }

        [Test]
        public void should_teardown_assembly_on_eviction_notification()
        {
            evictionNotification.OnNext(interviewId);

            Assert.Throws<KeyNotFoundException>(() =>
            {
                Manager.Execute(Create.Command.AnswerDateTimeQuestionCommand(interviewId, interviewerId, DateTime.Now));
            });
        }

        [TearDown]
        public void TearDown()
        {
            Manager.TearDown(interviewId);
        }
    }
}