using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.CloneMultiOptionQuestionHandlerTests
{
    internal class when_cloning_multi_option_question_and_variable_name_is_null : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new QRBarcodeQuestionAdded()
            {
                QuestionId = sourceQuestionId,
                ParentGroupId = chapterId,
                Title = "old title",
                VariableName = "old_variable_name",
                IsMandatory = false,
                Instructions = "old instructions",
                EnablementCondition = "old condition",
                ResponsibleId = responsibleId
            });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.CloneMultiOptionQuestion(
                    questionId: questionId,
                    title: title,
                    variableName: null,
                    variableLabel: null,
                    isMandatory: isMandatory,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    parentGroupId: parentGroupId,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex,
                    responsibleId: responsibleId,
                    options: options,
                    linkedToQuestionId: linkedToQuestionId,
                    areAnswersOrdered: areAnswersOrdered,
                    maxAllowedAnswers: maxAllowedAnswers));

        It should_not_throw_exception = () => exception.ShouldBeNull();


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid sourceQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid parentGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static int targetIndex = 1;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
        private static string validationExpression = null;
        private static string validationMessage = null;
        private static bool areAnswersOrdered = false;
        private static int? maxAllowedAnswers = null;
        private static Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), "2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
    }
}