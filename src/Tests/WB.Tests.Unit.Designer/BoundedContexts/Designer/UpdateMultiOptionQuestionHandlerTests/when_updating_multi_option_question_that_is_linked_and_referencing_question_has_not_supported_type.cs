using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateMultiOptionQuestionHandlerTests
{
    internal class when_updating_multi_option_question_that_is_linked_and_referencing_question_has_not_supported_type : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(parentGroupId, responsibleId: responsibleId);
            questionnaire.AddGroup(rosterId, parentGroupId, responsibleId: responsibleId, isRoster: true);
            
            questionnaire.AddGpsQuestion
            (
                linkedToQuestionId,
                rosterId,
                responsibleId,
                title : "text question",
                variableName : "source_of_linked_q"
            );
            questionnaire.AddQRBarcodeQuestion(questionId,
                        parentGroupId,
                        responsibleId,
                        title: "old title",
                        variableName: "old_variable_name",
                        instructions: "old instructions",
                        enablementCondition: "old condition");

            questionnaire.AddGroup(groupFromRosterId, rosterId, responsibleId: responsibleId);
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

        It should_throw_exception_with_message_containting__linked_question_type_number_date_text__ = () =>
            new[] { "linked question can be only type of number, text or date" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid groupFromRosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid parentGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static string variableName = "multi_var";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string enablementCondition = "";
        private static Option[] options = null;
        private static Guid linkedToQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static bool areAnswersOrdered = false;
        private static int? maxAllowedAnswers = null;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static bool yesNoView = false;
    }
}