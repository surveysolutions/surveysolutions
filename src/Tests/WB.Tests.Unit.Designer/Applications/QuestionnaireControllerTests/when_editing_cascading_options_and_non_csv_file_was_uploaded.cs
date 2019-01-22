using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using FluentAssertions;
using Moq;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Extensions;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options_and_non_csv_file_was_uploaded : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateQuestionnaireController();
            SetControllerContextWithSession(controller, "options", new QuestionnaireController.EditOptionsViewModel());

            var stream = new MemoryStream();

            var imageInBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAAA1BMVEX/TQBcNTh/AAAAAXRSTlPM0jRW/QAAAApJREFUeJxjYgAAAAYAAzY3fKgAAAAASUVORK5CYII=";
            var imageStream = new MemoryStream(Convert.FromBase64String(imageInBase64));
            Image.FromStream(imageStream).Save(stream, ImageFormat.Jpeg);

            stream.Position = 0;
            postedFile = Mock.Of<HttpPostedFileBase>(pf => pf.InputStream == stream);
            BecauseOf();
        }

        private void BecauseOf() => controller.EditCascadingOptions(postedFile);

        [NUnit.Framework.Test] public void should_add_error_message_to_temp_data () =>
            controller.TempData[Alerts.ERROR].Should().Be("Only tab-separated values files are accepted");

        private static QuestionnaireController controller;
        private static HttpPostedFileBase postedFile;
    }
}
