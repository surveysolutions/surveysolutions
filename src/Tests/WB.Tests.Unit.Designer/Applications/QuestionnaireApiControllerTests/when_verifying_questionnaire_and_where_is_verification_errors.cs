using System;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Tests.Unit.Designer.BoundedContexts.Designer.InterviewCompilerTests;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Models;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_verifying_questionnaire_and_where_is_verification_errors : QuestionnaireApiControllerTestContext
    {
        [OneTimeSetUp] public void context () {
            questionnaireDocument = CreateQuestionnaireDocument().AsReadOnly();
            questionnaireView = CreateQuestionnaireView(questionnaireDocument.Questionnaire);

            verificationMessages = new QuestionnaireVerificationMessage[]
            {
                Create.VerificationError("error1", "message1", Create.VerificationReference(Guid.NewGuid())),
                Create.VerificationError("error2", "message2",
                    Create.VerificationReference(Guid.NewGuid(), QuestionnaireVerificationReferenceType.Group)),
            };

            verificationWarnings = new QuestionnaireVerificationMessage[]
            {
                Create.VerificationWarning("code1", "message3",
                    Create.VerificationReference(Guid.NewGuid(), QuestionnaireVerificationReferenceType.Roster)),
                Create.VerificationWarning("code2", "message4",
                    Create.VerificationReference(Guid.NewGuid(), QuestionnaireVerificationReferenceType.Group)),
                Create.VerificationWarning("code3", "message5",
                    Create.VerificationReference(Guid.NewGuid(), QuestionnaireVerificationReferenceType.Question))
            };

            var allVerificationErrors = verificationMessages.Union(verificationWarnings);

            mappedAndEnrichedVerificationErrors = new VerificationMessage[]
            {
                Create.VerificationMessage("aaa", "aaaa",
                    Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Question,
                        Guid.NewGuid(), "aaaaaaaaaaaaaaaaaaaaaa")),
                Create.VerificationMessage("aaa", "aaaa",
                    Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Question,
                        Guid.NewGuid(), "aaaaaaaaaaaaaaaaaaaaaa")),
                Create.VerificationMessage("ccc", "ccccc",
                    Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Question,
                        Guid.NewGuid(), "ccccccccccccccccc")),
            };
            mappedAndEnrichedVerificationWarnings = new VerificationMessage[]
            {
                Create.VerificationMessage("ccc", "ccccc",
                    Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Question,
                        Guid.NewGuid(), "ccccccccccccccccc")),
                Create.VerificationMessage("ddd", "ddddd",
                    Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Group, Guid.NewGuid(),
                        "ccccccccccccccccc")),
                Create.VerificationMessage("eee", "eeeee",
                    Create.VerificationReferenceEnriched(QuestionnaireVerificationReferenceType.Question,
                        Guid.NewGuid(), "ccccccccccccccccc"))
            };

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(x => 
                x.Load(Moq.It.IsAny<QuestionnaireRevision>()) == questionnaireView);

            verifierMock = new Mock<IQuestionnaireVerifier>();

            verifierMock
                .Setup(x => x.GetAllErrors(questionnaireView, 
                    Moq.It.IsAny<bool>()))
                .Returns(allVerificationErrors);
            
            errorsMapperMock = new Mock<IVerificationErrorsMapper>();

            errorsMapperMock
                .Setup(x => x.EnrichVerificationErrors(verificationMessages, Moq.It.IsAny<ReadOnlyQuestionnaireDocument>()))
                .Returns(mappedAndEnrichedVerificationErrors);

            errorsMapperMock
               .Setup(x => x.EnrichVerificationErrors(verificationWarnings, Moq.It.IsAny<ReadOnlyQuestionnaireDocument>()))
               .Returns(mappedAndEnrichedVerificationWarnings);

            controller = CreateQuestionnaireController(
                questionnaireViewFactory: questionnaireViewFactory, 
                questionnaireVerifier: verifierMock.Object,
                verificationErrorsMapper: errorsMapperMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = (VerificationResult) (controller.Verify(questionnaireId) as OkObjectResult).Value;

        [Test] public void should_call_verifier_once () =>
            verifierMock.Verify(x => x.GetAllErrors(It.IsAny<QuestionnaireView>(),
                It.IsAny<bool>()), Times.Once);
        
        [Test] public void should_call_errors_mapper_once () =>
            errorsMapperMock.Verify(x => 
                x.EnrichVerificationErrors(verificationMessages, Moq.It.IsAny<ReadOnlyQuestionnaireDocument>()), Times.Once);

        [Test] public void should_return_messages_created_by_mapper_as_action_result () =>
            result.Errors.Should().BeEquivalentTo(mappedAndEnrichedVerificationErrors);

        [Test] public void should_return_warnings_created_by_mapper_as_action_result () =>
            result.Warnings.Should().BeEquivalentTo(mappedAndEnrichedVerificationWarnings);

        private static ReadOnlyQuestionnaireDocument questionnaireDocument; 
        private static QuestionnaireView questionnaireView; 
        private static Mock<IQuestionnaireVerifier> verifierMock ;
        private static Mock<IVerificationErrorsMapper> errorsMapperMock;
        private static QuestionnaireVerificationMessage[] verificationMessages;
        private static QuestionnaireVerificationMessage[] verificationWarnings;
        private static QuestionnaireApiController controller;
        private static VerificationMessage[] mappedAndEnrichedVerificationErrors;
        private static VerificationMessage[] mappedAndEnrichedVerificationWarnings;
        private static VerificationResult result;
    }
}
