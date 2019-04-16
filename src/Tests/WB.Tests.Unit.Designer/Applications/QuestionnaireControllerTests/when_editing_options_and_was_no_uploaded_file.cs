using FluentAssertions;
using WB.UI.Shared.Web.Extensions;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_options_and_was_no_uploaded_file : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.Test]
        public void should_add_error_message_to_temp_data()
        {
            var controller = CreateQuestionnaireController();

            controller.EditOptions(null);

            controller.TempData[Alerts.ERROR].Should().Be("Choose tab-separated values file to upload, please");
        }
    }
}
