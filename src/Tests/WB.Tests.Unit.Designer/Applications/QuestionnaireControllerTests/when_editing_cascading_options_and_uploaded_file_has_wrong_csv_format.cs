using System;
using System.IO;
using System.Web;
using Machine.Specifications;
using Moq;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Extensions;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options_and_uploaded_file_has_wrong_csv_format : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateQuestionnaireController();
            SetControllerContextWithSession(controller, "options", new QuestionnaireController.EditOptionsViewModel());

            stream = GenerateStreamFromString(Guid.NewGuid() + Environment.NewLine + Guid.NewGuid());

            stream.Position = 0;
            postedFile = Mock.Of<HttpPostedFileBase>(pf => pf.InputStream == stream && pf.FileName == "image.csv");
            BecauseOf();
        }

        private void BecauseOf() => controller.EditCascadingOptions(postedFile);

        [NUnit.Framework.Test] public void should_add_error_message_to_temp_data () =>
            controller.TempData[Alerts.ERROR].ShouldEqual("Only tab-separated values files are accepted");

        [NUnit.Framework.OneTimeTearDown]
        public void stuff() => stream.Dispose();

        private static QuestionnaireController controller;
        private static HttpPostedFileBase postedFile;
        private static Stream stream = new MemoryStream();
    }
}