using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

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

            QuestionnaireDocument questionnaire = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: textQuetionId, variable: "txt"),
                    Create.Entity.Group(Guid.NewGuid(), "Group X", null, "txt==\"Nastya\"", false, new[]
                    {
                        Create.Entity.Variable(variableId, VariableType.LongInteger, "v1", "txt.Length")
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
           eventContext.GetSingleEvent<VariablesEnabled>().Variables.ShouldContainOnly( Abc.Create.Identity(variableId, RosterVector.Empty));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid variableId;
        private static Guid textQuetionId;
    }
}