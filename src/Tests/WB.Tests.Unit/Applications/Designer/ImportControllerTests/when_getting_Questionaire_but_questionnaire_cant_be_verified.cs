using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Machine.Specifications;
using Machine.Specifications.Utility;
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
    internal class when_getting_Questionaire_but_questionnaire_cant_be_verified : ImportControllerTestContext
    {
        Establish context = () =>
        {
            request = Create.DownloadQuestionnaireRequest(questionnaireId);

            var membershipUserService = Mock
                .Of<IMembershipUserService>(
                    _ =>
                        _.WebUser == Mock.Of<IMembershipWebUser>(u => u.UserId == userId));

            var questionnaireViewFactory = Mock
                .Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(
                    _ =>
                        _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(userId));

            var expressionsEngineVersionService = Mock
                .Of<IExpressionsEngineVersionService>(
                    _ =>
                        _.IsClientVersionSupported(Moq.It.IsAny<Version>()) == true);

            var questionnaireVerifier = Mock
                .Of<IQuestionnaireVerifier>(
                    _ =>
                        _.Verify(Moq.It.IsAny<QuestionnaireDocument>()) ==
                        new[] {Create.QuestionnaireVerificationError()});

            importController = CreateImportController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory,
                expressionsEngineVersionService: expressionsEngineVersionService,
                questionnaireVerifier: questionnaireVerifier);
        };

        Because of = () =>
            exception = Catch.Only<HttpResponseException>(() =>
                importController.Questionnaire(request));

        It should_throw_HttpResponseException = () =>
            exception.ShouldBeOfExactType<HttpResponseException>();

        It should_throw_HttpResponseException_with_StatusCode_PreconditionFailed = () =>
            ((HttpResponseException)exception).Response.StatusCode.ShouldEqual(HttpStatusCode.PreconditionFailed);

        It should_throw_HttpResponseException_with_explanation_in_ReasonPhrase = () =>
            (new[] { "questionnaire", "errors", "verify"}).Each(x => ((HttpResponseException)exception).Response.ReasonPhrase.ToLower().ShouldContain(x));

        private static ImportController importController;
        private static Exception exception;
        private static DownloadQuestionnaireRequest request;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static Guid userId = Guid.Parse("33333333333333333333333333333333");
    }
}
