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
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Api;
using WB.UI.Shared.Web.Membership;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.ImportControllerTests
{
    internal class when_getting_Questionaire_but_client_version_is_not_supported : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);

            var membershipUserService = Mock
                .Of<IMembershipUserService>(
                    _ =>
                        _.WebUser == Mock
                            .Of<IMembershipWebUser>(
                                u =>
                                    u.UserId == userId));

            var questionnaireViewFactory = Mock
                .Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(
                    _ =>
                        _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(userId));

            var expressionsEngineVersionService = Mock
                .Of<IExpressionsEngineVersionService>(
                    _ =>
                        _.IsClientVersionSupported(Moq.It.IsAny<Version>()) == false);

            importController = CreateImportController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory,
                expressionsEngineVersionService: expressionsEngineVersionService);
        };

        Because of = () =>
            exception = Catch.Only<HttpResponseException>(() =>
                importController.Questionnaire(request));

        It should_throw_HttpResponseException = () =>
            exception.ShouldBeOfExactType<HttpResponseException>();

        It should_throw_HttpResponseException_with_StatusCode_UpgradeRequired = () =>
            ((HttpResponseException)exception).Response.StatusCode.ShouldEqual(HttpStatusCode.UpgradeRequired);

        It should_throw_HttpResponseException_with_explanation_in_ReasonPhrase = () =>
            ((HttpResponseException)exception).Response.ReasonPhrase.ShouldEqual(
                    "Failed to open the questionnaire. Your version is 0.0.0. Please update.");

        private static ImportController importController;
        private static Exception exception;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
    }
}
