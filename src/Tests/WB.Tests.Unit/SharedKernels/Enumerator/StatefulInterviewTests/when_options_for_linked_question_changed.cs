using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_options_for_linked_question_changed : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            linkSourceId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            linkedQuestionIdentity = Create.Entity.Identity(linkedQuestionId, RosterVector.Empty);
            newOptionsEvent = new[] {
                new ChangedLinkedOptions(linkedQuestionIdentity, 
                                         new []
                                         {
                                             Create.Entity.RosterVector(1),
                                             Create.Entity.RosterVector(2)
                                         })
            };

            var questionnaire =
                Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Entity.FixedRoster(
                        fixedTitles:
                            new[]
                            {
                                new FixedRosterTitle(1, "first fixed roster"),
                                new FixedRosterTitle(2, "second fixed roster")
                            },
                        children: new[]
                        {
                            Create.Entity.TextQuestion(linkSourceId)
                        }),
                    Create.Entity.MultyOptionsQuestion(linkedQuestionId, linkedToQuestionId: linkSourceId)
                }));

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.Apply(Create.Event.TextQuestionAnswered(linkSourceId, Create.Entity.RosterVector(1), "one"));
            interview.Apply(Create.Event.TextQuestionAnswered(linkSourceId, Create.Entity.RosterVector(2), "two"));
        };

        Because of = () => interview.Apply(Create.Event.LinkedOptionsChanged(newOptionsEvent));

        It should_calculate_state_of_options_for_linked_question = () =>
        {
            interview.GetLinkedMultiOptionQuestion(linkedQuestionIdentity)
                .Options.Count.ShouldEqual(2);
        };

        static StatefulInterview interview;
        static Guid linkedQuestionId;
        static Guid linkSourceId;
        static ChangedLinkedOptions[] newOptionsEvent;
        static Identity linkedQuestionIdentity;
    }
}