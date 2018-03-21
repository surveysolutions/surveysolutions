using System;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_assign_interview_on_same_interviewer : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewerId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            supervisorId = Guid.Parse("BBAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Mock.Of<IQuestionnaire>();

            var questionnaireRepository = 
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.AssignInterviewer(supervisorId, interviewerId, DateTime.Now);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = NUnit.Framework.Assert.Throws<InterviewException>(() => interview.AssignInterviewer(supervisorId, interviewerId, DateTime.Now));

        [NUnit.Framework.Test] public void should_contains_exseption_message () =>
            exception.Message.Should().NotBeNull();

        private static Guid interviewerId;
        private static Guid supervisorId;
        private static Guid questionnaireId;

        private static Exception exception;
        private static Interview interview;
    }
}
