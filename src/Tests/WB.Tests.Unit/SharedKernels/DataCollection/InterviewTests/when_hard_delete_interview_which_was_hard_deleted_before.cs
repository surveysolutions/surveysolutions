using System;
using System.Linq;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_hard_delete_interview_which_was_hard_deleted_before : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter());
            interview.Apply(new InterviewHardDeleted(userId, DateTimeOffset.Now));

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
                interview.HardDelete(userId, DateTimeOffset.Now);

        [NUnit.Framework.Test] public void should_raise_zero_events () =>
            eventContext.Events.Count().Should().Be(0);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static Guid userId;

        private static Guid questionnaireId;

        private static EventContext eventContext;
        private static Interview interview;
    }
}
