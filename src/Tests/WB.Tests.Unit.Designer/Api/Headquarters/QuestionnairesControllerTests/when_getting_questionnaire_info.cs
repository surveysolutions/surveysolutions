using System;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Controllers.Api.Headquarters;

namespace WB.Tests.Unit.Designer.Api.Headquarters.QuestionnairesControllerTests
{
    [TestOf(typeof(HQQuestionnairesController))]
    internal class when_getting_questionnaire_info : QuestionnairesControllerTestContext
    {
        [Test]
        public void should_mark_questionnaire_timestamps_as_utc()
        {
            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(
                _ => _.Load(It.IsAny<QuestionnaireViewInputModel>()) == Create.QuestionnaireView(userId));

            var listItemStorage = Create.InMemoryDbContext();
            listItemStorage.Questionnaires.Add(new QuestionnaireListViewItem
            {
                QuestionnaireId = questionnaireId.FormatGuid(),
                Title = "Title",
                CreationDate = createdAt,
                LastEntryDate = lastUpdatedAt
            });
            listItemStorage.SaveChanges();

            var controller = CreateQuestionnairesController(
                questionnaireViewFactory: questionnaireViewFactory,
                listItemStorage: listItemStorage);

            controller.SetupLoggedInUser(userId);

            var result = controller.Info(questionnaireId) as OkObjectResult;
            var questionnaireInfo = result?.Value as QuestionnaireInfo;

            Assert.That(questionnaireInfo, Is.Not.Null);
            Assert.That(questionnaireInfo!.CreatedAt, Is.EqualTo(createdAt));
            Assert.That(questionnaireInfo.CreatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(questionnaireInfo.LastUpdatedAt, Is.EqualTo(lastUpdatedAt));
            Assert.That(questionnaireInfo.LastUpdatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        private static readonly Guid questionnaireId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid userId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly DateTime createdAt = new DateTime(2024, 05, 07, 16, 30, 00, DateTimeKind.Unspecified);
        private static readonly DateTime lastUpdatedAt = new DateTime(2024, 05, 08, 07, 45, 00, DateTimeKind.Unspecified);
    }
}
