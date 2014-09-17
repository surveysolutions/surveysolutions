using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.UpdateMultimediaQuestionHandlerTests
{
    internal class when_updating_multimedia_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded()
            {
                QuestionType = QuestionType.Text,
                PublicKey = questionId,
                GroupPublicKey = chapterId,
                QuestionText = "old title",
                StataExportCaption = "old_variable_name",
                Mandatory = false,
                Instructions = "old instructions",
                ConditionExpression = "old condition",
                ResponsibleId = responsibleId
            });
            eventContext = new EventContext();
        };

        Because of = () =>
                questionnaire.UpdateMultimediaQuestion(questionId: questionId, title: "title",
                    variableName: "multimedia_question",
                    variableLabel: variableName, isMandatory: isMandatory, enablementCondition: condition, instructions: instructions,
                    responsibleId: responsibleId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_MultimediaQuestionUpdated_event = () =>
            eventContext.ShouldContainEvent<MultimediaQuestionUpdated>();

        It should_raise_MultimediaQuestionUpdated_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .QuestionId.ShouldEqual(questionId);

        It should_raise_MultimediaQuestionUpdated_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .VariableName.ShouldEqual(variableName);

        It should_raise_MultimediaQuestionUpdated_event_with_title_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .Title.ShouldEqual(title);

        It should_raise_MultimediaQuestionUpdated_event_with_condition_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .EnablementCondition.ShouldEqual(condition);

        It should_raise_MultimediaQuestionUpdated_event_with_ismandatory_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .IsMandatory.ShouldEqual(isMandatory);

        It should_raise_MultimediaQuestionUpdated_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .Instructions.ShouldEqual(instructions);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "multimedia_question";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
    }
}
