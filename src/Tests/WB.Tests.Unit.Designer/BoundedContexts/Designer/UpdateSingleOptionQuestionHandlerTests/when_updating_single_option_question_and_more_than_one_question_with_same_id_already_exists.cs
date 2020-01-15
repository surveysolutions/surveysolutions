using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_single_option_question_and_more_than_one_question_with_same_id_already_exists : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () {
            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var answers = new List<Answer>() { new Answer() { AnswerCode = 1, AnswerText = "1" }, new Answer() { AnswerCode = 2, AnswerText = "2" } };

            var questionnaireDoc = Create.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.SingleQuestion(id:questionId,isFilteredCombobox:false, options:answers),
                    Create.SingleQuestion(id:questionId,isFilteredCombobox:false, options:answers)
                });

            questionnaire = Create.Questionnaire();
            questionnaire.Initialize(Guid.NewGuid(), questionnaireDoc, null);
            BecauseOf();

            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] { "more", "question(s)", "exist" });
        }

        private void BecauseOf() =>
            exception = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.UpdateSingleOptionQuestion(
                    new UpdateSingleOptionQuestion(
                        questionnaireId: questionnaire.Id,
                        questionId: questionId,
                        commonQuestionParameters: new CommonQuestionParameters()
                        {
                            Title = title,
                            VariableName = variableName,
                            VariableLabel = null,
                            EnablementCondition = enablementCondition,
                            Instructions = instructions,
                            HideIfDisabled = false
                        },

                        isPreFilled: isPreFilled,
                        scope: scope,
                        responsibleId: responsibleId,
                        options: options,
                        linkedToEntityId: linkedToQuestionId,
                        isFilteredCombobox: isFilteredCombobox,
                        cascadeFromQuestionId: cascadeFromQuestionId,
                        validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                        linkedFilterExpression: null,
                        validationExpression: null,
                        validationMessage: null,
                        showAsList: false,
                        showAsListThreshold: null,
                        categoriesId: null)
                    ));

        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
        private static Option[] options = new Option[] { new Option("1", "Option 1"), new Option("2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
        private static bool isFilteredCombobox = false;
        private static Guid? cascadeFromQuestionId = (Guid?)null;
    }
}
