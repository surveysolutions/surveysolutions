using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.UI.Designer.Controllers;

using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context ()
        {
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");
            var comboboxQuestionId = Guid.Parse("12345678901234567890123456789012");
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: questionnaireId,
                children: new IComposite[]
                {
                    Create.SingleOptionQuestion(questionId: comboboxQuestionId, isComboBox: true,
                        answers: new[]
                            {
                                Create.Answer(value: 1m, answer: "a"),
                                Create.Answer(value: 2m, answer: "b")
                            }
                            .ToList()),
                    Create.SingleOptionQuestion(questionId: questionId, cascadeFromQuestionId: comboboxQuestionId),
                });

            controller = CreateQuestionnaireController(
                categoricalOptionsImportService: Create.CategoricalOptionsImportService(questionnaire));

            SetControllerContextWithSession(controller, "options", new QuestionnaireController.EditOptionsViewModel
            {
                QuestionnaireId = questionnaireId.FormatGuid(),
                QuestionId = questionId
            });

            stream = GenerateStreamFromString("1\tStreet 1\t2");

            stream.Position = 0;
            postedFile = Mock.Of<HttpPostedFileBase>(pf => pf.InputStream == stream && pf.FileName == "data.csv");
            BecauseOf();
        }

        private void BecauseOf() => view = controller.EditCascadingOptions(postedFile);

        [NUnit.Framework.Test] public void should_return_list_with_1_option () =>
            ((IEnumerable<QuestionnaireCategoricalOption>)view.Model).Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_first_option_with_value_equals_1 () =>
            ((IEnumerable<QuestionnaireCategoricalOption>)view.Model).First().Value.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_first_option_with_title_equals_Street_1 () =>
            ((IEnumerable<QuestionnaireCategoricalOption>)view.Model).First().Title.Should().Be("Street 1");

        [NUnit.Framework.Test]
        public void should_return_first_option_with_parent_value_equals_2() =>
            ((IEnumerable<QuestionnaireCategoricalOption>)view.Model).First().ParentValue.Should().Be(2);

        [OneTimeTearDown]
        public void cleanup()
        {
            stream.Dispose();
        }

        private static QuestionnaireController controller;
        private static HttpPostedFileBase postedFile;
        private static Stream stream = new MemoryStream();
        private static ViewResult view;
    }
}
