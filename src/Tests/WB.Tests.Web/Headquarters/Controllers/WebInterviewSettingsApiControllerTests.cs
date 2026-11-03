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
    }
}
