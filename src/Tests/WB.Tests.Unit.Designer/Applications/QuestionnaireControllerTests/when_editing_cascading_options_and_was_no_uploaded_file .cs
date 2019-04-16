using FluentAssertions;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Extensions;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options_and_was_no_uploaded_file : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateQuestionnaireController();
            BecauseOf();
        }

        private void BecauseOf() => controller.EditCascadingOptions(null);

        [NUnit.Framework.Test] public void should_add_error_message_to_temp_data () =>
            controller.TempData[Alerts.ERROR].Should().Be("Choose tab-separated values file to upload, please");

        private static QuestionnaireController controller;
    }
}
