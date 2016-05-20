using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_completing_interview_with_valid_static_texts : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            staticTextIdentity = Create.Other.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaire = Create.Other.PlainQuestionnaire(Create.Other.QuestionnaireDocument(questionnaireId,
                Create.Other.Group(children: new List<IComposite>()
                {
                    Create.Other.StaticText(staticTextIdentity.Id)
                })));

            var plainQuestionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            statefulInterview = Create.Other.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);

            statefulInterview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));
            statefulInterview.Apply(Create.Event.StaticTextsDeclaredInvalid(staticTextIdentity));
            statefulInterview.Apply(Create.Event.StaticTextsDeclaredValid(staticTextIdentity));

            eventContext = new EventContext();
        };

        Because of = () => statefulInterview.Complete(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), "", DateTime.Now);

        It should_raize_interview_declated_valid_event = () => eventContext.ShouldContainEvent<InterviewDeclaredValid>();
        It should_not_raize_interview_declated_invalid_event = () => eventContext.ShouldNotContainEvent<InterviewDeclaredInvalid>();

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
        static EventContext eventContext;
    }
}