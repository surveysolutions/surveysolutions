using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_verifying_questionnaire_and_where_is_verification_errors : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocument();
            questionnaireView = CreateQuestionnaireView(questionnaireDocument);

            verificationMessages =  new QuestionnaireVerificationMessage[]
            {
                Create.VerificationError("error1", "message1", Create.VerificationReference(Guid.NewGuid())),
                Create.VerificationError("error2", "message2", Create.VerificationReference(Guid.NewGuid(), QuestionnaireVerificationReferenceType.Group)),
            };

            verificationWarnings = new QuestionnaireVerificationMessage[]
            {
                Create.VerificationWarning("code1", "message3", Create.VerificationReference(Guid.NewGuid(), QuestionnaireVerificationReferenceType.Roster)),
                Create.VerificationWarning("code2", "message4", Create.VerificationReference(Guid.NewGuid(), QuestionnaireVerificationReferenceType.Group)),
                Create.VerificationWarning("code3", "message5", Create.VerificationReference(Guid.NewGuid(), QuestionnaireVerificationReferenceType.Question))
            };

            var allVerificationErrors = verificationMessages.Union(verificationWarnings);

            mappedAndEnrichedVerificationErrors = new VerificationMessage[]
            {
                Create.VerificationMessage("aaa","aaaa", Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Question, Guid.NewGuid(), "aaaaaaaaaaaaaaaaaaaaaa")),
                Create.VerificationMessage("aaa","aaaa", Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Question, Guid.NewGuid(), "aaaaaaaaaaaaaaaaaaaaaa")),
                Create.VerificationMessage("ccc","ccccc", Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Question, Guid.NewGuid(), "ccccccccccccccccc")),
            };
            mappedAndEnrichedVerificationWarnings = new VerificationMessage[]
            {
                Create.VerificationMessage("ccc","ccccc", Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Question, Guid.NewGuid(), "ccccccccccccccccc")),
                Create.VerificationMessage("ddd","ddddd", Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Group, Guid.NewGuid(), "ccccccccccccccccc")),
                Create.VerificationMessage("eee","eeeee", Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Question, Guid.NewGuid(), "ccccccccccccccccc"))
            };

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == questionnaireView);
            verifierMock = new Mock<IQuestionnaireVerifier>();

            verifierMock
                .Setup(x => x.Verify(questionnaireView))
                .Returns(allVerificationErrors);

            errorsMapperMock = new Mock<IVerificationErrorsMapper>();

            errorsMapperMock
                .Setup(x => x.EnrichVerificationErrors(verificationMessages, questionnaireDocument))
                .Returns(mappedAndEnrichedVerificationErrors);

            errorsMapperMock
               .Setup(x => x.EnrichVerificationErrors(verificationWarnings, questionnaireDocument))
               .Returns(mappedAndEnrichedVerificationWarnings);

            controller = CreateQuestionnaireController(
                questionnaireViewFactory: questionnaireViewFactory, 
                questionnaireVerifier: verifierMock.Object,
                verificationErrorsMapper: errorsMapperMock.Object);
        };

        Because of = () =>
            result = controller.Verify(questionnaireId);

        It should_call_verifier_once = () =>
            verifierMock.Verify(x => x.Verify(questionnaireView), Times.Once);

        It should_call_errors_mapper_once = () =>
            errorsMapperMock.Verify(x => x.EnrichVerificationErrors(verificationMessages, questionnaireDocument), Times.Once);

        It should_return_messages_created_by_mapper_as_action_result = () =>
            result.Errors.ShouldEqual(mappedAndEnrichedVerificationErrors);

        It should_return_warnings_created_by_mapper_as_action_result = () =>
            result.Warnings.ShouldEqual(mappedAndEnrichedVerificationWarnings);

        private static QuestionnaireDocument questionnaireDocument; 
        private static QuestionnaireView questionnaireView; 
        private static Mock<IQuestionnaireVerifier> verifierMock ;
        private static Mock<IVerificationErrorsMapper> errorsMapperMock;
        private static QuestionnaireVerificationMessage[] verificationMessages;
        private static QuestionnaireVerificationMessage[] verificationWarnings;
        private static QuestionnaireController controller;
        private static VerificationMessage[] mappedAndEnrichedVerificationErrors;
        private static VerificationMessage[] mappedAndEnrichedVerificationWarnings;
        private static VerificationResult result;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
    }
}