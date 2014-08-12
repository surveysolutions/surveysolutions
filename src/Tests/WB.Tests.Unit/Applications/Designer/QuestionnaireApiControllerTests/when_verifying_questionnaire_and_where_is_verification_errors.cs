using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.View;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireApiControllerTests
{
    internal class when_verifying_questionnaire_and_where_is_verification_errors : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocument();
            var questionnaireView = CreateQuestionnaireView(questionnaireDocument);
            verificationErrors = CreateQuestionnaireVerificationErrors();
            mappedAndEnrichedVerificationErrors = CreateVerificationErrors();

            var questionnaireViewFactory = Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == questionnaireView);
            verifierMock = new Mock<IQuestionnaireVerifier>();

            verifierMock
                .Setup(x => x.Verify(questionnaireDocument))
                .Returns(verificationErrors);

            errorsMapperMock = new Mock<IVerificationErrorsMapper>();

            errorsMapperMock
                .Setup(x => x.EnrichVerificationErrors(verificationErrors, questionnaireDocument))
                .Returns(mappedAndEnrichedVerificationErrors);

            controller = CreateQuestionnaireController(
                questionnaireViewFactory: questionnaireViewFactory, 
                questionnaireVerifier: verifierMock.Object,
                verificationErrorsMapper: errorsMapperMock.Object);
        };

        Because of = () =>
            result = controller.Verify(questionnaireId);

        It should_call_verifier_once = () =>
            verifierMock.Verify(x => x.Verify(questionnaireDocument), Times.Once);

        It should_call_errors_mapper_once = () =>
            errorsMapperMock.Verify(x => x.EnrichVerificationErrors(verificationErrors, questionnaireDocument), Times.Once);

        It should_return_errors_created_by_mapper_as_action_result = () =>
            result.Errors.ShouldEqual(mappedAndEnrichedVerificationErrors);

        private static QuestionnaireDocument questionnaireDocument; 
        private static Mock<IQuestionnaireVerifier> verifierMock ;
        private static Mock<IVerificationErrorsMapper> errorsMapperMock;
        private static QuestionnaireVerificationError[] verificationErrors;
        private static QuestionnaireController controller;
        private static VerificationError[] mappedAndEnrichedVerificationErrors;
        private static VerificationErrors result;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
    }
}