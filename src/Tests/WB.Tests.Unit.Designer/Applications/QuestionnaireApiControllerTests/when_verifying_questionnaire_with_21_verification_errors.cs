using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Models;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_verifying_questionnaire_with_21_verification_errors : QuestionnaireApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocument = CreateQuestionnaireDocument(
                new Group
                {
                    PublicKey = new Guid(),
                    Children = new IComposite[101].Select(_ => new TextQuestion {PublicKey = Guid.NewGuid()}).ToList<IComposite>().ToReadOnlyCollection()
                });
            questionnaireView = CreateQuestionnaireView(questionnaireDocument);

            verificationMessages = CreateQuestionnaireVerificationErrors(questionnaireDocument.Find<IQuestion>(_ => true));

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(x => x.Load(Moq.It.IsAny<QuestionnaireRevision>()) == questionnaireView);
            verifierMock = new Mock<IQuestionnaireVerifier>();

            string _;
            verifierMock
                .Setup(x => x.GetAllErrors(questionnaireView, 
                    Moq.It.IsAny<bool>()))
                .Returns(verificationMessages);

            controller = CreateQuestionnaireController(
                questionnaireViewFactory: questionnaireViewFactory, 
                questionnaireVerifier: verifierMock.Object,
                verificationErrorsMapper: new VerificationErrorsMapper());
            BecauseOf();
        }

        private void BecauseOf() =>
            result = (VerificationResult) (controller.Verify(questionnaireId) as OkObjectResult)?.Value;

        [NUnit.Framework.Test] public void should_returned_errors_contains_specified_errors_count () =>
            result.Errors.Sum(error => error.Errors.SelectMany(e => e.References).Count()).Should().Be(QuestionnaireApiController.MaxVerificationErrors);

        private static QuestionnaireDocument questionnaireDocument;
        private static QuestionnaireView questionnaireView;
        private static Mock<IQuestionnaireVerifier> verifierMock ;
        private static QuestionnaireVerificationMessage[] verificationMessages;
        private static QuestionnaireApiController controller;
        private static VerificationResult result;
    }
}
