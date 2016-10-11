using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateMultiOptionQuestionHandlerTests
{
    internal class when_updating_multi_option_question_and_more_than_one_question_with_same_id_already_exists : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var answers = new List<Answer>() { new Answer() { AnswerCode = 1, AnswerText = "1" }, new Answer() { AnswerCode = 2, AnswerText = "2" } };

            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaireDoc = Create.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.SingleQuestion(id:questionId,isFilteredCombobox:false, options:answers),
                    Create.SingleQuestion(id:questionId,isFilteredCombobox:false, options:answers)
                });

            questionnaire = Create.Questionnaire();
            questionnaire.Initialize(Guid.NewGuid(),questionnaireDoc,null);
           
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateMultiOptionQuestion(
                    questionId: questionId,
                    title: title,
                    variableName: variableName,
                    variableLabel: null,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    hideIfDisabled: false,
                    instructions: instructions,
                    responsibleId: responsibleId
                    , options: options,
                    linkedToEntityId: linkedToQuestionId,
                    areAnswersOrdered: areAnswersOrdered,
                    maxAllowedAnswers: maxAllowedAnswers,
                    yesNoView: yesNoView, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null, properties: Create.QuestionProperties()));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__more__question__exist__ = () =>
            new[] { "more", "question", "exist" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
        private static Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), "2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
        private static bool areAnswersOrdered = false;
        private static int? maxAllowedAnswers = null;
        private static bool yesNoView = false;
    }
}