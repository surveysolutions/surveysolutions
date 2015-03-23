using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateGpsCoordinatesQuestionHandlerTests
{
    internal class when_updating_gps_coordinates_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        private Establish context = () =>
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
            eventContext = new EventContext();
        };

        private Because of = () =>
            questionnaire.UpdateGpsCoordinatesQuestion(
                questionId: questionId,
                title: title,
                variableName: variableName,
                variableLabel: variableLabel,
                isMandatory: isMandatory,
                scope: scope,
                enablementCondition: enablementCondition,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId

    );

        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

         It should_raise_QuestionChanged_event = () =>
            eventContext.ShouldContainEvent<QuestionChanged>();

        It should_raise_QuestionChanged_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .PublicKey.ShouldEqual(questionId);

        It should_raise_QuestionChanged_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .StataExportCaption.ShouldEqual(variableName);

        It should_raise_QuestionChanged_event_with_variable_label_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .VariableLabel.ShouldEqual(variableLabel);

        It should_raise_QuestionChanged_event_with_title_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .QuestionText.ShouldEqual(title);

        It should_raise_QuestionChanged_event_with_condition_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .ConditionExpression.ShouldEqual(enablementCondition);

        It should_raise_QuestionChanged_event_with_ismandatory_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Mandatory.ShouldEqual(isMandatory);

        It should_raise_QuestionChanged_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Instructions.ShouldEqual(instructions);

        It should_raise_QuestionChanged_event_with_scope_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .QuestionScope.ShouldEqual(scope);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string variableLabel = "label";
        private static string instructions = "intructions";
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = "some condition";
        private static string validationExpression = "some expression";
        private static string validationMessage = "some message";
    }
}