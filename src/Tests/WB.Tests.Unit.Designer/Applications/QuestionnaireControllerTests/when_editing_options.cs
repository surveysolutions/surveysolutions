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
            {
                QuestionnaireId = questionnaireId.FormatGuid(),
                QuestionId = questionId,
                IsCascading = true
            };
            BecauseOf();
        }

        private void BecauseOf() => view = controller.EditOptions(postedFile) as JsonResult;

        [NUnit.Framework.Test] public void should_return_0_errors () =>
            ((List<string>)view.Value).Should().BeEmpty();

        [NUnit.Framework.OneTimeTearDown]
        public void cleanup()
        {
            stream.Dispose();
        }

        private static QuestionnaireController controller;
        private static IFormFile postedFile;
        private static Stream stream = new MemoryStream();
        private static JsonResult view;
    }
}
