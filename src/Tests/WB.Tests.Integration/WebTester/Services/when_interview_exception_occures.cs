using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Integration.WebTester.Services
{
    public class when_interview_exception_occures : AppdomainsPerInterviewManagerTestsBase
    {
        private AppdomainsPerInterviewManager manager;
        private Guid interviewId = Id.gA;
        private Guid numericQuestionId = Id.gB;
        private Guid interviewerId = Id.g1;

        [SetUp]
        public void Setup()
        {
            manager = CreateManager();
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
        public void should_return_exception_to_caller()
        {
            var exception = Assert.Throws<AnswerNotAcceptedException>(() =>
                manager.Execute(Create.Command.AnswerTextQuestionCommand(interviewId, interviewId, questionId: numericQuestionId, answer: "answer")));

            Assert.That(exception.ExceptionType, Is.EqualTo(InterviewDomainExceptionType.AnswerNotAccepted));
        }

        [TearDown]
        public void TearDown()
        {
            manager.TearDown(interviewId);
        }
    }
}