using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api;
using WB.UI.Shared.Web.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.ImportControllerTests
{
    internal class when_call_Questionnaire_method_but_questionnaire_compilation_throw_exception : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = new DownloadQuestionnaireRequest()
            {
                QuestionnaireId = questionnaireId,
                SupportedVersion = new QuestionnnaireVersion()
            };

            var membershipUserService =
                Mock.Of<IMembershipUserService>(
                    _ => _.WebUser == Mock.Of<IMembershipWebUser>(u => u.UserId == userId));

            var questionnaireViewFactory =
                Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(
                    _ =>
                        _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) ==
                        new QuestionnaireView(new QuestionnaireDocument() { CreatedBy = userId }));

            var expressionsEngineVersionService =
                Mock.Of<IExpressionsEngineVersionService>(
                    _ => _.IsClientVersionSupported(Moq.It.IsAny<Version>()) == true);

            var questionnaireVerifier =
                Mock.Of<IQuestionnaireVerifier>(
                    _ =>
                        _.Verify(Moq.It.IsAny<QuestionnaireDocument>()) == new QuestionnaireVerificationError[0]);

            string generatedAssembly;
            var expressionProcessorGenerator = new Mock<IExpressionProcessorGenerator>();
            expressionProcessorGenerator.Setup(
                x => x.GenerateProcessorStateAssembly(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<Version>(),
                    out generatedAssembly)).Throws<NullReferenceException>();

            importController = CreateImportController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory,
                expressionsEngineVersionService: expressionsEngineVersionService,
                questionnaireVerifier: questionnaireVerifier,
                expressionProcessorGenerator: expressionProcessorGenerator.Object);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                importController.Questionnaire(request));

        It should_throw_HttpResponseException = () =>
            exception.ShouldBeOfExactType<HttpResponseException>();

        It should_throw_HttpResponseException_with_StatusCode_UpgradeRequired = () =>
            ((HttpResponseException)exception).Response.StatusCode.ShouldEqual(HttpStatusCode.UpgradeRequired);

        It should_throw_HttpResponseException_with_explanation_in_ReasonPhrase = () =>
            ((HttpResponseException)exception).Response.ReasonPhrase.ShouldEqual(
                    "Your questionnaire \"\" contains new functionality which is not supported by your installation. Please update.");

        private static ImportController importController;
        private static Exception exception;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
    }
}
