using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_single_option_question_that_is_linked_and_referecing_question_not_under_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = parentGroupId });
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = parentGroupId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.AddTextQuestion(linkedToQuestionId,
                parentGroupId,
                responsibleId,
                title : "text question",
                variableName: "source_of_linked_question"
            );
            questionnaire.AddQRBarcodeQuestion(
                questionId,
                parentGroupId,
                responsibleId,
                title: "old title",
                variableName: "old_variable_name",
                instructions: "old instructions");
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupFromRosterId, ParentGroupPublicKey = rosterId });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateSingleOptionQuestion(
                    questionId: questionId,
                    title: title,
                    variableName: variableName,
                    variableLabel: null,
                    isPreFilled: isPreFilled,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    hideIfDisabled: false,
                    instructions: instructions,
                    responsibleId: responsibleId,
                    options: options,
                    linkedToEntityId: linkedToQuestionId,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: ñascadeFromQuestionId, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null, properties: Create.QuestionProperties()));


        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__roster_pre_filled_ = () =>
            new[] { "question that you are linked to is not in the roster group" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid groupFromRosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid parentGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static bool isPreFilled = false;
        private static string variableName = "single_var";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string enablementCondition = "";
        private static Option[] options = null;
        private static Guid linkedToQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static bool isFilteredCombobox = false;
        private static Guid? ñascadeFromQuestionId = (Guid?)null;
    }
}