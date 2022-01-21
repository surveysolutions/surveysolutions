using System;
using FluentAssertions;
using Main.Core.Documents;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Controllers.Api.Headquarters;


namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_and_questionnaire_has_warnings_only : ImportControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);
            var dbContext = Create.InMemoryDbContext();
            dbContext.AddUserWithId(userId);

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(userId));

            var expressionsEngineVersionService = Setup.DesignerEngineVersionService();

            var questionnaireVerifier = Mock.Of<IQuestionnaireVerifier>(
                _ => _.GetAllErrors(Moq.It.IsAny<QuestionnaireView>(), It.IsAny<bool>()) ==
                     new[] { Create.VerificationWarning("code", "message") });

            string generatedAssembly = "test";
            var expressionProcessorGenerator = Mock.Of<IExpressionProcessorGenerator>(
                _ =>
                    _.GenerateProcessorStateAssembly(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>(),
                        out generatedAssembly) == Create.GenerationResult(true));

            importController = CreateImportController(
                questionnaireViewFactory: questionnaireViewFactory,
                engineVersionService: expressionsEngineVersionService,
                questionnaireVerifier: questionnaireVerifier,
                expressionProcessorGenerator: expressionProcessorGenerator);

            importController.SetupLoggedInUser(userId);
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaireCommunicationPackage = importController.Questionnaire(request);

        [NUnit.Framework.Test] public void should_return_not_null_response () =>
            questionnaireCommunicationPackage.Should().NotBeNull();

        private static ImportV2Controller importController;
        private static DownloadQuestionnaireRequest request;
        private static readonly Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid userId = Guid.Parse("33333333333333333333333333333333");
        private static IActionResult questionnaireCommunicationPackage;
    }
}
