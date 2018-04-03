using System;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_completing_interview_with_linked_questions : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            linkedQuestionIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            Guid linkedSourceQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            var questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.SingleOptionQuestion(linkedQuestionIdentity.Id, linkedToQuestionId: linkedSourceQuestionId),
                    Create.Entity.FixedRoster(children: new []
                    {
                        Create.Entity.TextQuestion(linkedSourceQuestionId)
                    })
                ));

            var plainQuestionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);

            statefulInterview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));
            statefulInterview.Apply(Create.Event.LinkedOptionsChanged(new[] {Create.Entity.ChangedLinkedOptions(linkedQuestionIdentity.Id, linkedQuestionIdentity.RosterVector, new [] {Create.Entity.RosterVector(0)})}));
            statefulInterview.Apply(Create.Event.LinkedOptionsChanged(new[] { Create.Entity.ChangedLinkedOptions(linkedQuestionIdentity.Id, linkedQuestionIdentity.RosterVector, new[] { Create.Entity.RosterVector(0), Create.Entity.RosterVector(1)})}));
            statefulInterview.Apply(Create.Event.LinkedOptionsChanged(new[] { Create.Entity.ChangedLinkedOptions(linkedQuestionIdentity.Id, linkedQuestionIdentity.RosterVector, new[] { Create.Entity.RosterVector(0), Create.Entity.RosterVector(1), Create.Entity.RosterVector(2)})}));

            eventContext = new EventContext();

            BecauseOf();
        }

        private void BecauseOf() => statefulInterview.Complete(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), "", DateTime.Now);

        [NUnit.Framework.Test] public void should_raize_linked_option_changed_aggregated_event_event () => eventContext.ShouldContainEvent<LinkedOptionsChanged>();

        static StatefulInterview statefulInterview;
        static Identity linkedQuestionIdentity;
        static EventContext eventContext;
    }
}
