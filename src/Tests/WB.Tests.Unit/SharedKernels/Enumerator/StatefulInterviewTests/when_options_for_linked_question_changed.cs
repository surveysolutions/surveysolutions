using System;
using System.Linq;
using Machine.Specifications;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Ignore("KP-8159")]
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

            IQuestionnaire questionnaire = Substitute.For<IQuestionnaire>();
            questionnaire.GetQuestionReferencedByLinkedQuestion(linkedQuestionId)
                .Returns(linkSourceId);

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.Apply(Create.Event.TextQuestionAnswered(linkSourceId, Create.Entity.RosterVector(1), "one"));
            interview.Apply(Create.Event.TextQuestionAnswered(linkSourceId, Create.Entity.RosterVector(2), "two"));
        };

        Because of = () => interview.Apply(Create.Event.LinkedOptionsChanged(newOptionsEvent));

        It should_calculate_state_of_options_for_linked_question = () =>
        {
            //var answers = interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkSourceId, linkedQuestionIdentity);
            //answers.Count().ShouldEqual(2);
        };

        static StatefulInterview interview;
        static Guid linkedQuestionId;
        static Guid linkSourceId;
        static ChangedLinkedOptions[] newOptionsEvent;
        static Identity linkedQuestionIdentity;
    }
}