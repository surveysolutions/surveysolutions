using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Extensions;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_options_and_was_no_uploaded_file : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.Test]
        public void should_add_error_message_to_temp_data()
        {
            var controller = CreateQuestionnaireController();
            controller.questionWithOptionsViewModel = new QuestionnaireController.EditOptionsViewModel(Id.g1.FormatGuid(), Id.g2)
            {
                IsCascading = true
            };
            var result = (JsonResult)controller.EditOptions(null);

            ((List<string>) result.Value)[0].Should().Be("Choose tab-separated values file to upload, please");
        }
    }
}
