using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;

using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_inside_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
            questionnaire.AddTextQuestion(questionAId, chapterId, responsibleId: responsibleId);
            questionnaire.AddTextQuestion(questionBId, chapterId, responsibleId: responsibleId);
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


        It should_contains_Question_with_QuestionId_specified_on_second_place = () =>
            questionnaire.QuestionnaireDocument.Find<Group>(chapterId).Children[1]
                .PublicKey.ShouldEqual(questionId);

        
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionAId = Guid.Parse("A1111111111111111111111111111111");
        private static Guid questionBId = Guid.Parse("B1111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "text_question";
        private static bool isPreFilled = false;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string enablementCondition = "enablementCondition";
        private static string validationExpression = "validationExpression";
        private static string validationMessage = "validationMessage";
        private static int index = 1;
    }
}