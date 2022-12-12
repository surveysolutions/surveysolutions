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
using NHibernate.Criterion;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
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

            stream = GenerateStreamFromString("1\tStreet 1\r\n2\tStreet 2");

            stream.Position = 0;
            postedFile = Mock.Of<IFormFile>(pf => pf.OpenReadStream() == stream && pf.FileName == "data.csv");
            BecauseOf();
        }

        private void BecauseOf() => view =  controller.EditOptions(new QuestionnaireRevision(Id.g1), Id.g2, postedFile).Value;

        [NUnit.Framework.Test]
        public void should_return_no_errors() =>
           view.Errors.Count.Should().Be(0);

        [NUnit.Framework.Test]
        public void should_add_two_option() =>
            view.Options.Length.Should().Be(2);

        [NUnit.Framework.Test]
        public void should_add_first_option_with_expected_value() =>
            view.Options.First().Title.Should().Equals("First");

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
