using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Extensions;
using Image = System.Drawing.Image;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options_and_non_csv_file_was_uploaded : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateQuestionnaireController();

            var stream = new MemoryStream();

            var imageInBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAAA1BMVEX/TQBcNTh/AAAAAXRSTlPM0jRW/QAAAApJREFUeJxjYgAAAAYAAzY3fKgAAAAASUVORK5CYII=";
            var imageStream = new MemoryStream(Convert.FromBase64String(imageInBase64));
            Image.FromStream(imageStream).Save(stream, ImageFormat.Jpeg);

            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");

            stream.Position = 0;
            postedFile = Mock.Of<IFormFile>(pf => pf.OpenReadStream() == stream);
            controller.questionWithOptionsViewModel = new QuestionnaireController.EditOptionsViewModel(
                questionnaireId: questionnaireId.FormatGuid(),
                questionId: questionId,
                options: new List<QuestionnaireCategoricalOption>()
            );

            controller.questionWithOptionsViewModel = new QuestionnaireController.EditOptionsViewModel(questionnaireId.FormatGuid(), questionId)
            {
                IsCascading = false
            };

            BecauseOf();
        }

        private void BecauseOf() => result = (JsonResult)controller.EditOptions(postedFile);

        [NUnit.Framework.Test] public void should_add_error_message_to_temp_data () =>
            ((List<string>)result.Value)[0].Should().Be("Only tab-separated values files are accepted");

        private static QuestionnaireController controller;
        private static IFormFile postedFile;
        private static JsonResult result;
    }
}
