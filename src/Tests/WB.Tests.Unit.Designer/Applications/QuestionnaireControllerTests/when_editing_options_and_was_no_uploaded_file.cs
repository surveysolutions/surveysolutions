using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_options_and_was_no_uploaded_file : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.Test]
        public void should_add_error_message_to_temp_data()
        {
            var controller = CreateQuestionnaireController();
            var result = controller.EditOptions(new QuestionnaireRevision(Id.g1), Id.g2, null);
            
            ClassicAssert.NotNull(result.Value.Errors);
            result.Value.Errors[0].Should().Be("Choose tab-separated values file to upload, please");
        }
    }
}
