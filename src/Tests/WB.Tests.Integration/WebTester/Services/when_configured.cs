using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Integration.WebTester.Services
{
    public class when_configured : AppdomainsPerInterviewManagerTestsBase
    {
        private AppdomainsPerInterviewManager manager;
        private Guid interviewId;

        [SetUp]
        public void Setup()
        {
            manager = CreateManager();
            interviewId = Id.gA;
        }

        [Test]
        public void should_be_able_to_execute_simple_command_in_app_domain()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion());
            questionnaire.IsUsingExpressionStorage = true;
            questionnaire.ExpressionsPlayOrder = Create.Service.ExpressionsPlayOrderProvider().GetExpressionsPlayOrder(new ReadOnlyQuestionnaireDocument(questionnaire));

            var supportingAssembly = IntegrationCreate.CompileAssembly(questionnaire);

            // act
            manager.SetupForInterview(interviewId, questionnaire, null, supportingAssembly);
            var events = manager.Execute(new CreateInterview(interviewId, Id.g1,
                Create.Entity.QuestionnaireIdentity(questionnaire.PublicKey, 1),
                new List<InterviewAnswer>(),
                new List<string>(), 
                Guid.NewGuid(),
                null,
                Create.Entity.InterviewKey(),
                null));

            // assert
            Assert.That(events, Is.Not.Empty);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Assert.That(assembly.FullName, Does.Not.Contain($"rules-{questionnaire.PublicKey:N}"), "Hosting app domain should not load rules assembly");
            }

            var rules = Assembly.Load(Convert.FromBase64String(supportingAssembly));
            Assert.That(rules.FullName, Does.StartWith($"rules-{questionnaire.PublicKey:N}"), "Recheck previous assert because naming rules for assebmly has changed and it might not catch that assembly was loaded when it shouldn't");
        }

        [TearDown]
        public void TearDown() => manager.TearDown(interviewId);
    }
}
