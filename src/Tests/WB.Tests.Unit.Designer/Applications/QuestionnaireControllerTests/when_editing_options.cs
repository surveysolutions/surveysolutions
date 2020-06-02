using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.GenericSubdomains.Portable;
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
            controller.questionWithOptionsViewModel = new QuestionnaireController.EditOptionsViewModel
            (
                questionnaireId : questionnaireId.FormatGuid(),
                questionId : questionId,
                options:new QuestionnaireCategoricalOption[0]
            );
            BecauseOf();
        }

        private void BecauseOf() => view = controller.EditOptions(postedFile) as ViewResult;

        [NUnit.Framework.Test] public void should_return_list_with_1_option () =>
            ((QuestionnaireController.EditOptionsViewModel)view.Model).Options.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_first_option_with_value_equals_1 () =>
            ((QuestionnaireController.EditOptionsViewModel)view.Model).Options.First().Value.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_first_option_with_title_equals_Street_1 () =>
            ((QuestionnaireController.EditOptionsViewModel)view.Model).Options.First().Title.Should().Be("Street 1");

        [NUnit.Framework.OneTimeTearDown]
        public void cleanup()
        {
            stream.Dispose();
        }

        private static QuestionnaireController controller;
        private static IFormFile postedFile;
        private static Stream stream = new MemoryStream();
        private static ViewResult view;
    }
}
