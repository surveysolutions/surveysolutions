﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_single_option_question_with_linkedQuestion_and_supervisor_scope : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = parentGroupId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = linkedToQuestionId,
                GroupPublicKey = rosterId,
                QuestionType = QuestionType.Text,
                QuestionText = "text question",
                StataExportCaption = "source_of_linked_question"
            });
            questionnaire.Apply(new QRBarcodeQuestionAdded
            {
                QuestionId = questionId,
                ParentGroupId = parentGroupId,
                Title = "old title",
                VariableName = "old_variable_name",
                Instructions = "old instructions",
                EnablementCondition = "old condition",
                ResponsibleId = responsibleId
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupFromRosterId, ParentGroupPublicKey = rosterId });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateSingleOptionQuestion(
                    questionId: questionId,
                    title: title,
                    variableName: variableName,
                    variableLabel: null,
                    isPreFilled: isPreFilled,
                    scope: QuestionScope.Supervisor,
                    enablementCondition: enablementCondition,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId,
                    options: null,
                    linkedToQuestionId: linkedToQuestionId,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: сascadeFromQuestionId
                    ));


        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting_linked_categorical_questions_cannot_be_filled_by_supervisor_ = () =>
            new[] { "linked categorical questions cannot be filled by supervisor" }.ShouldEachConformTo(
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
        private static string validationExpression = "";
        private static string validationMessage = "";
        private static Guid linkedToQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static bool isFilteredCombobox = false;
        private static Guid? сascadeFromQuestionId = (Guid?)null;
    }
}