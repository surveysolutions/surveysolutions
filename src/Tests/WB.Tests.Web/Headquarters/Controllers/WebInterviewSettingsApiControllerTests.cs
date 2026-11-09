using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.UI.Headquarters.Controllers.Api.WebInterview;

namespace WB.Tests.Web.Headquarters.Controllers
{
    [TestFixture]
    [TestOf(typeof(WebInterviewSettingsApiController))]
    public class WebInterviewSettingsApiControllerTests
    {
        [Test]
        public void when_complete_screen_text_exceeds_limit_should_reject_page_template()
        {
            var controller = new WebInterviewSettingsApiController(
                Mock.Of<IWebInterviewConfigProvider>(),
                Mock.Of<IQuestionnaireBrowseViewFactory>());
            var model = new WebInterviewSettingsApiController.UpdatePageTemplateModel
            {
                MessageType = WebInterviewUserMessages.CompleteScreenText,
                MessageText = new string('x', 2001)
            };

            var result = controller.UpdatePageTemplate("invalid-id", model);

            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public void when_complete_screen_title_exceeds_limit_should_reject_page_template()
        {
            var controller = new WebInterviewSettingsApiController(
                Mock.Of<IWebInterviewConfigProvider>(),
                Mock.Of<IQuestionnaireBrowseViewFactory>());
            var model = new WebInterviewSettingsApiController.UpdatePageTemplateModel
            {
                TitleType = WebInterviewUserMessages.CompleteScreenText,
                TitleText = new string('x', 2001)
            };

            var result = controller.UpdatePageTemplate("invalid-id", model);

            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public void when_complete_screen_button_exceeds_limit_should_reject_page_template()
        {
            var controller = new WebInterviewSettingsApiController(
                Mock.Of<IWebInterviewConfigProvider>(),
                Mock.Of<IQuestionnaireBrowseViewFactory>());
            var model = new WebInterviewSettingsApiController.UpdatePageTemplateModel
            {
                ButtonType = WebInterviewUserMessages.CompleteScreenText,
                ButtonText = new string('x', 2001)
            };

            var result = controller.UpdatePageTemplate("invalid-id", model);

            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public void when_complete_screen_text_exceeds_limit_should_reject_page_message()
        {
            var controller = new WebInterviewSettingsApiController(
                Mock.Of<IWebInterviewConfigProvider>(),
                Mock.Of<IQuestionnaireBrowseViewFactory>());
            var model = new WebInterviewSettingsApiController.UpdatePageMessageModel
            {
                Type = WebInterviewUserMessages.CompleteScreenText,
                Message = new string('x', 2001)
            };

            var result = controller.UpdatePageMessage("invalid-id", model);

            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public void when_complete_screen_text_is_html_encoded_and_within_limit_should_not_reject_page_message()
        {
            var controller = new WebInterviewSettingsApiController(
                Mock.Of<IWebInterviewConfigProvider>(),
                Mock.Of<IQuestionnaireBrowseViewFactory>());
            var model = new WebInterviewSettingsApiController.UpdatePageMessageModel
            {
                Type = WebInterviewUserMessages.CompleteScreenText,
                Message = new string('\'', 2000).Replace("'", "&#39;")
            };

            var result = controller.UpdatePageMessage("invalid-id", model);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void when_complete_screen_text_contains_encoded_double_quotes_should_not_reject_page_message()
        {
            var controller = new WebInterviewSettingsApiController(
                Mock.Of<IWebInterviewConfigProvider>(),
                Mock.Of<IQuestionnaireBrowseViewFactory>());
            var model = new WebInterviewSettingsApiController.UpdatePageMessageModel
            {
                Type = WebInterviewUserMessages.CompleteScreenText,
                Message = new string('"', 2000).Replace("\"", "&quot;")
            };

            var result = controller.UpdatePageMessage("invalid-id", model);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void when_complete_screen_text_exceeds_stored_limit_should_reject_page_message()
        {
            var controller = new WebInterviewSettingsApiController(
                Mock.Of<IWebInterviewConfigProvider>(),
                Mock.Of<IQuestionnaireBrowseViewFactory>());
            var model = new WebInterviewSettingsApiController.UpdatePageMessageModel
            {
                Type = WebInterviewUserMessages.CompleteScreenText,
                Message = string.Concat(Enumerable.Repeat("&eacute;", 1501))
            };

            var result = controller.UpdatePageMessage("invalid-id", model);

            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public void when_complete_screen_text_contains_an_entity_not_decoded_by_lodash_and_exceeds_limit_should_reject_page_message()
        {
            var controller = new WebInterviewSettingsApiController(
                Mock.Of<IWebInterviewConfigProvider>(),
                Mock.Of<IQuestionnaireBrowseViewFactory>());
            var model = new WebInterviewSettingsApiController.UpdatePageMessageModel
            {
                Type = WebInterviewUserMessages.CompleteScreenText,
                Message = string.Concat(Enumerable.Repeat("&eacute;", 1500))
            };

            var result = controller.UpdatePageMessage("invalid-id", model);

            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }
    }
}
