using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;
using WB.Tests.Integration.InterviewTests;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Integration.WebTester.Services
{
    [TestOf(typeof(AppdomainsPerInterviewManager))]
    public class AppdomainsPerInterviewManagerTests : InterviewTestsContext
    {
        [Test]
        public void should_be_able_to_execute_simple_command_in_app_domain()
        {
            var manager = CreateManager();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion());
            questionnaire.IsUsingExpressionStorage = true;
            questionnaire.ExpressionsPlayOrder = Create.Service.ExpressionsPlayOrderProvider().GetExpressionsPlayOrder(new ReadOnlyQuestionnaireDocument(questionnaire));

            manager.SetupForInterview(Id.gA, questionnaire, IntegrationCreate.CompileAssembly(questionnaire));

            var events = manager.Execute(new CreateInterview(Id.gA, Id.g1,
                Create.Entity.QuestionnaireIdentity(questionnaire.PublicKey, 1),
                new List<InterviewAnswer>(),
                DateTime.UtcNow,
                Guid.NewGuid(),
                null,
                Create.Entity.InterviewKey(),
                null));

            Assert.That(events, Is.Not.Empty);
        }

        AppdomainsPerInterviewManager CreateManager()
        {
            var bin = Path.GetDirectoryName(typeof(AppdomainsPerInterviewManagerTests).Assembly.Location);
            return new AppdomainsPerInterviewManager(bin);
        }
    }
}