using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateDateTimeQuestionHandlerTests
{
    internal class when_updating_datetime_question_and_title_contains_substitution_to_question_from_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddNumericQuestion(
                rosterSizeQuestionId,
                chapterId,responsibleId,
                isInteger : true,
                variableName : "roster_size_question");

            questionnaire.AddQRBarcodeQuestion(
                    questionId,
                    chapterId,
                    responsibleId,
                    title: "old title",
                    variableName: "old_variable_name",
                    instructions: "old instructions",
                    enablementCondition: "old condition");

            questionnaire.AddGroup(rosterId,chapterId, responsibleId: responsibleId, isRoster: true, rosterSourceType: RosterSizeSourceType.Question,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: null);

            questionnaire.AddNumericQuestion(questionFromRosterId, rosterId, responsibleId,
                isInteger: true,variableName: substitutionVariableName);
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.UpdateDateTimeQuestion(command));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__title__contains_illegal__substitution__ = () =>
            new[] { "text", "contains", "illegal", "substitution" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionFromRosterId = Guid.Parse("22222222222222222222222222222222");
        private static Guid rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string substitutionVariableName = "substitution_var";
        private static string titleWithSubstitution = string.Format("title with substitution - %{0}%", substitutionVariableName);
        private static string variableName = "qr_barcode_question";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;

        private static readonly UpdateDateTimeQuestion command = new UpdateDateTimeQuestion(
            questionnaireId: Guid.Parse("22222222222222222222222222222222"),
            questionId: questionId,
            isPreFilled: isPreFilled,
            scope: scope,
            responsibleId: responsibleId,
            validationConditions: new List<ValidationCondition>(),
            commonQuestionParameters: new CommonQuestionParameters
            {
                Title = titleWithSubstitution,
                VariableName = variableName,
                VariableLabel = null,
                EnablementCondition = enablementCondition,
                HideIfDisabled = false,
                Instructions = instructions
            },
            isTimestamp: false);
    }
}