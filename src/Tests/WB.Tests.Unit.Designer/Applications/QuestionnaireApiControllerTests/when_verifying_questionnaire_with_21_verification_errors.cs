using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_verifying_questionnaire_with_21_verification_errors : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocument(
                new Group()
                {
                    PublicKey = new Guid(),
                    Children = new IComposite[101].Select(_ => new TextQuestion() {PublicKey = new Guid()}).ToList<IComposite>().ToReadOnlyCollection()
                });
            var questionnaireView = CreateQuestionnaireView(questionnaireDocument);

            verificationMessages = CreateQuestionnaireVerificationErrors(questionnaireDocument.Find<IComposite>(_ => true));

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == questionnaireView);
            verifierMock = new Mock<IQuestionnaireVerifier>();

            verifierMock
                .Setup(x => x.Verify(questionnaireDocument))
                .Returns(verificationMessages);

            controller = CreateQuestionnaireController(
                questionnaireViewFactory: questionnaireViewFactory, 
                questionnaireVerifier: verifierMock.Object,
                verificationErrorsMapper: new VerificationErrorsMapper());
        };

        Because of = () =>
            result = controller.Verify(questionnaireId);

        It should_returned_errors_contains_specified_errors_count = () =>
            result.Errors.Sum(error => error.Errors.SelectMany(e => e.References).Count()).ShouldEqual(QuestionnaireController.MaxVerificationErrors);

        private static QuestionnaireDocument questionnaireDocument; 
        private static Mock<IQuestionnaireVerifier> verifierMock ;
        private static QuestionnaireVerificationMessage[] verificationMessages;
        private static QuestionnaireController controller;
        private static VerificationResult result;
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
    }
}