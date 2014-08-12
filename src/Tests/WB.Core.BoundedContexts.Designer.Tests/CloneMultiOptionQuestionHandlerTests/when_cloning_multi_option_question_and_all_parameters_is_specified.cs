using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.CloneMultiOptionQuestionHandlerTests
{
    internal class when_cloning_multi_option_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });
            questionnaire.Apply(new QRBarcodeQuestionAdded
            {
                QuestionId = sourceQuestionId,
                ParentGroupId = parentGroupId,
                Title = "old title",
                VariableName = "old_variable_name",
                IsMandatory = false,
                Instructions = "old instructions",
                EnablementCondition = "old condition",
                ResponsibleId = responsibleId
            });
            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.CloneMultiOptionQuestion(
                questionId: questionId,
                title: title,
                variableName: variableName,
                variableLabel: null,
                isMandatory: isMandatory,
                scope: scope,
                enablementCondition: enablementCondition,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                parentGroupId: parentGroupId,
                sourceQuestionId: sourceQuestionId,
                targetIndex: targetIndex,
                responsibleId: responsibleId,
                options: options,
                linkedToQuestionId: linkedToQuestionId,
                areAnswersOrdered: areAnswersOrdered,
                maxAllowedAnswers: maxAllowedAnswers

                );

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionCloned_event = () =>
            eventContext.ShouldContainEvent<QuestionCloned>();

        It should_raise_QuestionCloned_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .PublicKey.ShouldEqual(questionId);

        It should_raise_QuestionCloned_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .GroupPublicKey.ShouldEqual(parentGroupId);

        It should_raise_QuestionCloned_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .StataExportCaption.ShouldEqual(variableName);

        It should_raise_QuestionCloned_event_with_title_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .QuestionText.ShouldEqual(title);

        It should_raise_QuestionCloned_event_with_condition_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .ConditionExpression.ShouldEqual(enablementCondition);

        It should_raise_QuestionCloned_event_with_ismandatory_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .Mandatory.ShouldEqual(isMandatory);

        It should_raise_QuestionCloned_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .Instructions.ShouldEqual(instructions);

        It should_raise_QuestionCloned_event_with_SourceQuestionId_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .SourceQuestionId.ShouldEqual(sourceQuestionId);

        It should_raise_QuestionCloned_event_with_TargetIndex_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .TargetIndex.ShouldEqual(targetIndex);

        It should_raise_NewQuestionAdded_event_with_same_options_count_as_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .Answers.Length.ShouldEqual(options.Length);

        It should_raise_NewQuestionAdded_event_with_same_option_titles_as_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .Answers.Select(x => x.AnswerText).ShouldContainOnly(options.Select(x => x.Title));

        It should_raise_NewQuestionAdded_event_with_same_option_values_as_specified = () =>
           eventContext.GetSingleEvent<QuestionCloned>()
               .Answers.Select(x => x.AnswerValue).ShouldContainOnly(options.Select(x => x.Value));

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid sourceQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid parentGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "datetime_question";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static int targetIndex = 1;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = "enablementCondition";
        private static string validationExpression = "validationExpression";
        private static string validationMessage = "validationMessage";
        private static bool areAnswersOrdered = false;
        private static int? maxAllowedAnswers = null;
        private static Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), "2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;

    }
}