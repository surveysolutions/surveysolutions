using System;
using System.Net;
using System.Web.Http;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Api.Headquarters;

namespace WB.Tests.Unit.Designer.Api.Headquarters.QuestionnairesControllerTests
{
    internal class when_getting_Questionaire_but_client_version_lower_than_serrver_one : QuestionnairesControllerTestContext
    {
        [Test] public void should_throw_HttpResponseException_with_explanation_in_ReasonPhrase () {
            var membershipUserService =
                Mock.Of<IMembershipUserService>(
                    _ => _.WebUser == Mock.Of<IMembershipWebUser>(u => u.UserId == userId));

            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(
                _ => _.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(userId));

            newQuestionnaireFeatureDescription = "variables";
            var expressionsEngineVersionService = Mock.Of<IDesignerEngineVersionService>(
                _ => _.IsClientVersionSupported(Moq.It.IsAny<int>()) == true && 
                     _.GetListOfNewFeaturesForClient(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>()) == new[] {newQuestionnaireFeatureDescription});

            questionnairesController = CreateQuestionnairesController(membershipUserService: membershipUserService,
                questionnaireViewFactory: questionnaireViewFactory,
                engineVersionService: expressionsEngineVersionService);

            var exception = Assert.Throws<HttpResponseException>(() => questionnairesController.Get(questionnaireId, 12, null));

            Assert.That(exception.Response.StatusCode, Is.EqualTo(HttpStatusCode.ExpectationFailed));

            exception.Response.ReasonPhrase.ToLower().ToSeparateWords().Should().Contain("questionnaire", "contains",
                "functionality", "not", "supported", "update",
                $"\"{newQuestionnaireFeatureDescription}\"");
        }

        private static HQQuestionnairesController questionnairesController;
        private static HttpResponseException exception;
        private static readonly Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid userId = Guid.Parse("33333333333333333333333333333333");
        private static string newQuestionnaireFeatureDescription;
    }
}
