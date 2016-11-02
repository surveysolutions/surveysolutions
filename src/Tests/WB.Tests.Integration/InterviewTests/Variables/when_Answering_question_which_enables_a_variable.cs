using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    internal class when_Answering_question_which_enables_a_variable : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            textQuetionId = Guid.Parse("21111111111111111111111111111111");
            variableId = Guid.Parse("22222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.TextQuestion(id: textQuetionId, variable: "txt"),
                    Create.Group(id: Guid.NewGuid(), enablementCondition: "txt==\"Nastya\"", children: new[]
                    {
                        Create.Variable(id: variableId, variableName: "v1", expression: "txt.Length")
                    })
                });

            interview = SetupInterview(questionnaireDocument: questionnaire);
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.AnswerTextQuestion(userId, textQuetionId, new decimal[0], DateTime.Now, "Nastya");

        It should_raise_VariablesValuesChanged_event_for_the_variable_with_value_equal_to_6 = () =>
           eventContext.ShouldContainEvent<VariablesChanged>(@event
               => (long?)@event.ChangedVariables[0].NewValue == 6 && @event.ChangedVariables[0].Identity.Id == variableId);

        It should_raise_VariablesDisabled_event_for_the_variable = () =>
           eventContext.GetSingleEvent<VariablesEnabled>().Variables.ShouldContainOnly( Create.Identity(variableId, Empty.RosterVector));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid variableId;
        private static Guid textQuetionId;
    }
}