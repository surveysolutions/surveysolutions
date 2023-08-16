using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.Test] public void should_return_stored_options()
        {
            var questionnaireId = Abc.Id.g1;
            var questionId = Abc.Id.g2;
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

            var controller = CreateQuestionnaireController(
                categoricalOptionsImportService: Create.CategoricalOptionsImportService(questionnaire));

            var stream = GenerateStreamFromString("1\tStreet 1\t2\r\n2\tStreet 2\t2");

            stream.Position = 0;
            var postedFile = Mock.Of<IFormFile>(pf => pf.OpenReadStream() == stream && pf.FileName == "data.csv");

            var view = controller.EditOptions(new QuestionnaireRevision(questionnaireId), questionId, postedFile);

            Assert.That(view.Value.Errors, Is.Empty);
        }
    }
}
