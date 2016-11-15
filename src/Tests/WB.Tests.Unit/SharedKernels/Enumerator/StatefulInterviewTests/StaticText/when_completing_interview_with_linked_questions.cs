using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    [Ignore("KP-8159")]
    internal class when_completing_interview_with_linked_questions : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(questionnaireId,
                Create.Entity.Group(children: new List<IComposite>()
                {
                    Create.Entity.SingleOptionQuestion(linkedQuestionIdentity.Id)
                })));

            var plainQuestionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);

            statefulInterview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));
            statefulInterview.Apply(Create.Event.LinkedOptionsChanged(new[] {Create.Entity.ChangedLinkedOptions(linkedQuestionIdentity.Id, linkedQuestionIdentity.RosterVector, new RosterVector[0])}));
            statefulInterview.Apply(Create.Event.LinkedOptionsChanged(new[] { Create.Entity.ChangedLinkedOptions(linkedQuestionIdentity.Id, linkedQuestionIdentity.RosterVector, new RosterVector[0]) }));
            statefulInterview.Apply(Create.Event.LinkedOptionsChanged(new[] { Create.Entity.ChangedLinkedOptions(linkedQuestionIdentity.Id, linkedQuestionIdentity.RosterVector, new RosterVector[0]) }));

            eventContext = new EventContext();
        };

        Because of = () => statefulInterview.Complete(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), "", DateTime.Now);

        It should_raize_linked_option_changed_aggregated_event_event = () => eventContext.ShouldContainEvent<LinkedOptionsChanged>();

        static StatefulInterview statefulInterview;
        static Identity linkedQuestionIdentity;
        static EventContext eventContext;
    }
}