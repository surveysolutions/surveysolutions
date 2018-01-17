using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Tests.Abc;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Integration.WebTester.Services
{
    public class when_interview_eviction_occures : AppdomainsPerInterviewManagerTestsBase
    {
        private AppdomainsPerInterviewManager manager;
        private Guid interviewId = Id.gA;
        private Guid numericQuestionId = Id.gB;
        private Guid interviewerId = Id.g1;
        private Subject<Guid> evictionNotification = new Subject<Guid>();

        [SetUp]
        public void Setup()
        {
            manager = CreateManager(evictionNotification);

            var numericIntegerQuestion = Create.Entity.NumericIntegerQuestion(id: numericQuestionId);
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(numericIntegerQuestion);
            questionnaire.IsUsingExpressionStorage = true;
            questionnaire.ExpressionsPlayOrder = Create.Service.ExpressionsPlayOrderProvider().GetExpressionsPlayOrder(new ReadOnlyQuestionnaireDocument(questionnaire));

            var supportingAssembly = IntegrationCreate.CompileAssembly(questionnaire);
            manager.SetupForInterview(interviewId, questionnaire, supportingAssembly);
            manager.Execute(Create.Command.CreateInterview(interviewId: interviewId,
                userId: interviewerId,
                questionnaireIdentity: Create.Entity.QuestionnaireIdentity(questionnaire.PublicKey, 1)));
        }

        [Test]
        public void should_teardown_assembly_on_eviction_notification()
        {
            evictionNotification.OnNext(interviewId);

            Assert.Throws<KeyNotFoundException>(() =>
            {
                manager.Execute(Create.Command.AnswerDateTimeQuestionCommand(interviewId, interviewerId, DateTime.Now));
            });
        }

        [TearDown]
        public void TearDown()
        {
            manager.TearDown(interviewId);
        }
    }
}