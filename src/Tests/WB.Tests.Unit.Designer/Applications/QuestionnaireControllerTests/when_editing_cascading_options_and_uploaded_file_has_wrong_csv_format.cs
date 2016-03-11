using System;
using System.IO;
using System.Web;
using Machine.Specifications;
using Moq;
using WB.UI.Designer.BootstrapSupport;
using WB.UI.Designer.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options_and_uploaded_file_has_wrong_csv_format : QuestionnaireControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
            SetControllerContextWithSession(controller, "options", new QuestionnaireController.EditOptionsViewModel());

            stream = GenerateStreamFromString(Guid.NewGuid() + Environment.NewLine + Guid.NewGuid());

            stream.Position = 0;
            postedFile = Mock.Of<HttpPostedFileBase>(pf => pf.InputStream == stream && pf.FileName == "image.csv");
        };

        Because of = () => controller.EditCascadingOptions(postedFile);

        It should_add_error_message_to_temp_data = () =>
            controller.TempData[Alerts.ERROR].ShouldEqual("Only tab-separated values files are accepted");

        Cleanup stuff = () => stream.Dispose();

        private static QuestionnaireController controller;
        private static HttpPostedFileBase postedFile;
        private static Stream stream = new MemoryStream();
    }
}