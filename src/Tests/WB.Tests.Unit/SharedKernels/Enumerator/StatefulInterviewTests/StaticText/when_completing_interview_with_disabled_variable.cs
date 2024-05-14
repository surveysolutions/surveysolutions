using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_completing_interview_with_disabled_variable : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            variableIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(questionnaireId,
                children: Create.Entity.Group(children: new List<IComposite>()
                {
                    Create.Entity.Variable(variableIdentity.Id)
                })));

            var plainQuestionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);

            statefulInterview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));
            statefulInterview.Apply(Create.Event.VariablesChanged(Create.Event.ChangedVariable(variableIdentity,"a")));
            statefulInterview.Apply(Create.Event.VariablesDisabled(variableIdentity));

            eventContext = new EventContext();
            BecauseOf();
        }

        private void BecauseOf() => statefulInterview.Complete(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), "", DateTime.Now, null);

        [NUnit.Framework.Test] public void should_raize_variable_disabled_event () => eventContext.ShouldContainEvent<VariablesDisabled>(v=>v.Variables[0]==variableIdentity);
        [NUnit.Framework.Test] public void should_not_raize_variable_enabled_event () => eventContext.ShouldNotContainEvent<VariablesEnabled>();
        [NUnit.Framework.Test] public void should_raize_variable_changed_event () => eventContext.ShouldContainEvent<VariablesChanged>(v => v.ChangedVariables[0].Identity == variableIdentity);

        static StatefulInterview statefulInterview;
        static Identity variableIdentity;
        static EventContext eventContext;
    }
}
