using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Controllers;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class when_editing_cascading_options : QuestionnaireControllerTestContext
    {
        [NUnit.Framework.Test] public void should_return_stored_options()
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

            var controller = CreateQuestionnaireController(
                categoricalOptionsImportService: Create.CategoricalOptionsImportService(questionnaire));
            
            controller.questionWithOptionsViewModel =
                new QuestionnaireController.EditOptionsViewModel
                (
                    questionnaireId : questionnaireId.FormatGuid(),
                    questionId : questionId,
                    options:new QuestionnaireCategoricalOption[0]
                );

            var stream = GenerateStreamFromString("1\tStreet 1\t2");

            stream.Position = 0;
            var postedFile = Mock.Of<IFormFile>(pf => pf.OpenReadStream() == stream && pf.FileName == "data.csv");

            var view = controller.EditOptions(postedFile) as JsonResult;

            var model = (List<string>)view.Value;
            model.Should().BeEmpty();
        }
    }
}
