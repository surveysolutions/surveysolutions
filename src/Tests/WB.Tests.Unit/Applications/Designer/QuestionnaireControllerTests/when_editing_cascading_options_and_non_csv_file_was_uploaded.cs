using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using Machine.Specifications;
using Moq;
using WB.UI.Designer.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options_and_non_csv_file_was_uploaded : QuestionnaireControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
            SetControllerContextWithSession(controller, "options", new QuestionnaireController.EditOptionsViewModel());

            stream = new MemoryStream();
            bmp = new Bitmap(10, 10);

            var graphics = Graphics.FromImage(bmp);
            graphics.FillRectangle(Brushes.Orange, 0, 0, 10, 10);
            bmp.Save(stream, ImageFormat.Jpeg);

            stream.Position = 0;
            postedFile = Mock.Of<HttpPostedFileBase>(pf => pf.InputStream == stream && pf.FileName == "image.jpeg");
        };

        Because of = () => controller.EditCascadingOptions(postedFile);

        It should_add_error_message_to_temp_data = () =>
            controller.TempData["error"].ShouldEqual("Only .csv (comma-separated values) files are accepted");

        Cleanup stuff = () =>
        {
            stream.Dispose();
            bmp.Dispose();
        };

        private static QuestionnaireController controller;
        private static HttpPostedFileBase postedFile;
        private static Stream stream = new MemoryStream();
        private static Bitmap bmp;
    }
}