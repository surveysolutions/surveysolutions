using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.AddGpsCoordinatesQuestionHandlerTests
{
    internal class when_adding_gps_coordinates_question_and_variable_name_not_unique : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = Guid.NewGuid(),
                GroupPublicKey = chapterId,
                StataExportCaption = notUniqueVariableName
            });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddGpsCoordinatesQuestion(
                    questionId: questionId,
                    parentGroupId: chapterId,
                    title: title,
                    variableName: notUniqueVariableName,
                variableLabel: null,
                    isMandatory: isMandatory,
                    scope: QuestionScope.Interviewer,
                    enablementCondition: enablementCondition,
                    instructions: instructions,
                    responsibleId: responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__variable__should__unique__ = () =>
            new[] { "variable", "should", "unique" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string notUniqueVariableName = "var1";
        private static bool isMandatory = false;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string enablementCondition = "";
    }
}