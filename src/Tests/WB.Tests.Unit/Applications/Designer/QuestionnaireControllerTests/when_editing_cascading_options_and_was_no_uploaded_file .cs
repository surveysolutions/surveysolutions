using Machine.Specifications;
using WB.UI.Designer.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options_and_was_no_uploaded_file : QuestionnaireControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
            SetControllerContextWithSession(controller, "options", new QuestionnaireController.EditOptionsViewModel());
        };

        Because of = () => controller.EditCascadingOptions(null);

        It should_add_error_message_to_temp_data = () =>
            controller.TempData["error"].ShouldEqual("Choose .csv (comma-separated values) file to upload, please");

        private static QuestionnaireController controller;
    }
}
