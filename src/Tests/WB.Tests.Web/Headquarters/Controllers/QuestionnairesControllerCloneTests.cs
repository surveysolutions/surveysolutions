using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters
{
    [TestFixture]
    [TestOf(typeof(QuestionnairesController))]
    public class QuestionnairesControllerCloneTests
    {
        [Test]
        public void when_cloning_questionnaire_should_copy_web_interview_settings()
        {
            var questionnaireId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var sourceIdentity = new QuestionnaireIdentity(questionnaireId, 1);
            var clonedIdentity = new QuestionnaireIdentity(questionnaireId, 2);
            var sourceConfig = new WebInterviewConfig
            {
                QuestionnaireId = sourceIdentity,
                Started = true,
                UseCaptcha = true,
                AllowSwitchToCawiForInterviewer = true,
            };
            var configProvider = new Mock<IWebInterviewConfigProvider>();
            configProvider.Setup(provider => provider.Get(sourceIdentity)).Returns(sourceConfig);

            var versionProvider = new Mock<IQuestionnaireVersionProvider>();
            versionProvider.Setup(provider => provider.GetNextVersion(questionnaireId)).Returns(2);

            var authorizedUser = new Mock<IAuthorizedUser>();
            authorizedUser.Setup(user => user.Id).Returns(userId);

            var controller = new QuestionnairesController(
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IQuestionnaireBrowseViewFactory>(),
                configProvider.Object,
                Mock.Of<IRestServiceSettings>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnairePdf>>(),
                Mock.Of<IUserViewFactory>(),
                versionProvider.Object,
                authorizedUser.Object,
                Mock.Of<ICommandService>(),
                Mock.Of<ILogger<QuestionnairesController>>(),
                Mock.Of<IPlainStorageAccessor<QuestionnaireCompositeItem>>());

            controller.Clone(new CloneQuestionnaireModel
            {
                Id = questionnaireId,
                Version = 1,
                NewTitle = "Cloned questionnaire",
            });

            configProvider.Verify(provider => provider.Store(
                clonedIdentity,
                It.Is<WebInterviewConfig>(config =>
                    config.Started &&
                    config.UseCaptcha &&
                    config.AllowSwitchToCawiForInterviewer)), Times.Once);
        }
    }
}
