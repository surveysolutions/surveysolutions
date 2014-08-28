using System.Drawing;
using System.IO;
using System.Web;
using Machine.Specifications;
using Moq;
using WB.UI.Designer.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireControllerTests
{
    internal class when_editing_options_and_uploaded_file_has_wrong_csv_format : QuestionnaireControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
            SetControllerContextWithSession(controller, "options", new QuestionnaireController.EditOptionsViewModel());

            stream = GenerateStreamFromString("kafnvkanfkvnefv30293r09qwreflqlr");

            stream.Position = 0;
            postedFile = Mock.Of<HttpPostedFileBase>(pf => pf.InputStream == stream && pf.FileName == "image.csv");
        };

        Because of = () => controller.EditOptions(postedFile);

        It should_add_error_message_to_temp_data = () =>
            controller.TempData["error"].ShouldEqual("CSV-file has wrong format or file is corrupted.");

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