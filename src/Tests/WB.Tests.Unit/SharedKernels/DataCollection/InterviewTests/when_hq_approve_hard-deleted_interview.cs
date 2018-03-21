using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_hq_approve_hard_deleted_interview : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter());

            interview.HardDelete(userId);

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
            exception =  NUnit.Framework.Assert.Throws<InterviewException>(() =>
                interview.HqApprove(userId,"my commet"));

        [NUnit.Framework.Test] public void should_raise_InterviewException () =>
            exception.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_raise_InterviewException_with_type_InterviewHardDeleted () =>
            exception.ExceptionType.Should().Be(InterviewDomainExceptionType.InterviewHardDeleted);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static Guid userId;

        private static Guid questionnaireId;

        private static EventContext eventContext;
        private static InterviewException exception;
        private static Interview interview;
    }
}
