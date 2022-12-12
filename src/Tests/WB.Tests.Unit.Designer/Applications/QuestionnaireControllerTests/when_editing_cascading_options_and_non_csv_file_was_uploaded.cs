using System;
using System.Drawing.Imaging;
using System.IO;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Controllers;
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
            stream.Position = 0;
            postedFile = Mock.Of<IFormFile>(pf => pf.OpenReadStream() == stream);

            BecauseOf();
        }

        private void BecauseOf() => result = controller.EditOptions(new QuestionnaireRevision(Abc.Id.g1), Abc.Id.g2, postedFile).Value;

        [NUnit.Framework.Test] public void should_add_error_message_to_temp_data () =>
            result.Errors[0].Should().Be("Only tab-separated values (*.tab, *.txt, *.tsv) or excel (*.xslx, *.xls, *.ods) files are accepted");

        private static QuestionnaireController controller;
        private static IFormFile postedFile;
        private static QuestionnaireController.EditOptionsResponse result;
    }
}
