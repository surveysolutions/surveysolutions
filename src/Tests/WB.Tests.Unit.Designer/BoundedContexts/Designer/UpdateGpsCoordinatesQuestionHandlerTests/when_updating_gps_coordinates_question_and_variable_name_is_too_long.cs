using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateGpsCoordinatesQuestionHandlerTests
{
    internal class when_updating_gps_coordinates_question_and_variable_name_is_too_long : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(Create.Event.NewQuestionAdded(
                    publicKey: questionId,
                    groupPublicKey: chapterId,
                    questionText: "old title",
                    stataExportCaption: "old_variable_name",
                    instructions: "old instructions",
                    conditionExpression: "old condition",
                    responsibleId: responsibleId,
                    questionType: QuestionType.QRBarcode
            ));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateGpsCoordinatesQuestion(
                    questionId: questionId,
                    title: title,
                    variableName: longVariableName,
                    variableLabel: null,
                    isPreFilled: false,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    hideIfDisabled: false,
                    instructions: instructions,
                    responsibleId: responsibleId, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>()));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__long__variable__ = () =>
            new[] { "long", "name" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string longVariableName = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private static string title = "title";
        private static string instructions = "intructions";
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
    }
}