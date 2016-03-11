using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateMultiOptionQuestionHandlerTests
{
    internal class when_updating_multi_option_question_with_linkedQuestion_and_supervisor_scope : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = parentGroupId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.Apply(Create.Event.NewQuestionAdded
            (
                publicKey : linkedToQuestionId,
                groupPublicKey : rosterId,
                questionType : QuestionType.Text,
                questionText : "text question",
                stataExportCaption : "source_of_linked_question"
            ));
            questionnaire.Apply(Create.Event.NewQuestionAdded(
                publicKey: questionId,
                groupPublicKey: parentGroupId,
                questionText: "old title",
                stataExportCaption: "old_variable_name",
                instructions: "old instructions",
                conditionExpression: "old condition",
                responsibleId: responsibleId,
                questionType: QuestionType.QRBarcode
                ));
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupFromRosterId, ParentGroupPublicKey = rosterId });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateMultiOptionQuestion(
                    questionId: questionId,
                    title: title,
                    variableName: variableName,
                    variableLabel: null,
                    scope: QuestionScope.Supervisor,
                    enablementCondition: enablementCondition,
                    hideIfDisabled: false,
                    instructions: instructions,
                    responsibleId: responsibleId,
                    options: null,
                    linkedToEntityId: linkedToQuestionId,
                    areAnswersOrdered: areAnswersOrdered,
                    maxAllowedAnswers: maxAllowedAnswers,
                    yesNoView: yesNoView, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null));


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
        private static string variableName = "multi_var";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string enablementCondition = "";
        private static Guid linkedToQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static bool areAnswersOrdered = false;
        private static int? maxAllowedAnswers = null;
        private static bool yesNoView = false;
    }
}