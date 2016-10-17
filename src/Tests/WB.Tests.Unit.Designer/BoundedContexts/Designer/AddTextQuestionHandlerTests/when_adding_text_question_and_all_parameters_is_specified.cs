using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Aggregates;

using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
        };

        Because of = () =>
            questionnaire.AddTextQuestion(
                questionId: questionId,
                parentId: chapterId,
                title: title,
                variableName: variableName,
                variableLabel: null,
                isPreFilled: isPreFilled,
                scope: QuestionScope.Interviewer,
                enablementCondition: enablementCondition,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                mask:null,
                responsibleId: responsibleId,
                index: index);


        It should_contains_Question_with_QuestionId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<TextQuestion>(questionId)
                .PublicKey.ShouldEqual(questionId);

        It should_contains_NewQuestion_with_ParentGroupId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(chapterId)
                .Children.ShouldContain(q => q.PublicKey == questionId);

        It should_contains_NewQuestion_with_title_specified = () =>
            questionnaire.QuestionnaireDocument.Find<TextQuestion>(questionId)
                .QuestionText.ShouldEqual(title);

        It should_contains_NewQuestion_with_variable_name_specified = () =>
            questionnaire.QuestionnaireDocument.Find<TextQuestion>(questionId)
                .StataExportCaption.ShouldEqual(variableName);

        It should_contains_NewQuestion_with_enablementCondition_specified = () =>
            questionnaire.QuestionnaireDocument.Find<TextQuestion>(questionId)
                .ConditionExpression.ShouldEqual(enablementCondition);

        It should_contains_NewQuestion_with_ifeatured_specified = () =>
            questionnaire.QuestionnaireDocument.Find<TextQuestion>(questionId)
                .Featured.ShouldEqual(isPreFilled);

        It should_contains_NewQuestion_with_instructions_specified = () =>
            questionnaire.QuestionnaireDocument.Find<TextQuestion>(questionId)
                .Instructions.ShouldEqual(instructions);


        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "text_question";
        private static bool isPreFilled = false;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string enablementCondition = "enablementCondition";
        private static string validationExpression = "validationExpression";
        private static string validationMessage = "validationMessage";
        private static int index = 5;
    }
}