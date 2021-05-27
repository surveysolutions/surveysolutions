using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    internal class when_Answering_integer_question_which_changes_value_of_long_variable : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();

            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            quetionId = Guid.Parse("21111111111111111111111111111111");
            variableId = Guid.Parse("22222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(quetionId, variable: "q1"),
                    Create.Entity.Variable(variableId, VariableType.LongInteger, "v1", "q1 * int.MinValue")
                });

            interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaire);
            eventContext = new EventContext();

            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
            appDomainContext.Dispose();
        }

        public void BecauseOf() =>
            interview.AnswerNumericIntegerQuestion(userId, quetionId, new decimal[0], DateTime.Now, int.MaxValue);

        [NUnit.Framework.Test] public void should_raise_VariablesValuesChanged_event_for_the_variable () =>
            eventContext.ShouldContainEvent<VariablesChanged>(@event
                => (long?) @event.ChangedVariables[0].NewValue == -4611686016279904256L && @event.ChangedVariables[0].Identity.Id== variableId);

        private static AppDomainContext appDomainContext;
        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid variableId;
        private static Guid quetionId;
    }
}
