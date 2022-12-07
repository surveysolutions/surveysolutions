using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Microsoft.AspNetCore.Http;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Tests.Abc;
using WB.UI.Designer.Controllers;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_options : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: questionnaireId,
                children: new IComposite[]
                {
                    Create.SingleOptionQuestion(questionId: questionId),
                });

            controller = CreateQuestionnaireController(
                categoricalOptionsImportService: Create.CategoricalOptionsImportService(questionnaire));

            stream = GenerateStreamFromString("1\tStreet 1");

            stream.Position = 0;
            postedFile = Mock.Of<IFormFile>(pf => pf.OpenReadStream() == stream && pf.FileName == "data.csv");
            BecauseOf();
        }

        private void BecauseOf() => view =  controller.EditOptions(new QuestionnaireRevision(Id.g1), Id.g2, postedFile).Value;

        [NUnit.Framework.Test]
        public void should_return_no_errors() =>
           view.Errors.Count.Should().Be(0);

        [NUnit.Framework.Test]
        public void should_add_one_option() =>
            view.Options.Length.Should().Be(1);

        [NUnit.Framework.Test]
        public void should_add_one_option_with_expected_value() =>
            view.Options.Single().Title.Equals("First");

        [NUnit.Framework.OneTimeTearDown]
        public void cleanup()
        {
            stream.Dispose();
        }

        private static QuestionnaireController controller;
        private static IFormFile postedFile;
        private static Stream stream = new MemoryStream();
        private static QuestionnaireController.EditOptionsResponse view;
    }
}
