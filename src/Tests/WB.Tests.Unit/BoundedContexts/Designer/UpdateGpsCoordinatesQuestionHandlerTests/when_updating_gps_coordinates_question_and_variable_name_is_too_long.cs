using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateGpsCoordinatesQuestionHandlerTests
{
    internal class when_updating_gps_coordinates_question_and_variable_name_is_too_long : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new QRBarcodeQuestionAdded()
            {
                QuestionId = questionId,
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
                questionnaire.UpdateGpsCoordinatesQuestion(
                    questionId: questionId,
                    title: title,
                    variableName: longVariableName,
                    variableLabel: null,
                    isMandatory: isMandatory,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    validationExpression: string.Empty,
                    validationMessage: string.Empty,
                    instructions: instructions,
                    responsibleId: responsibleId

                    ));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__long__variable__ = () =>
            new[] { "long", "variable" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string longVariableName = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
    }
}