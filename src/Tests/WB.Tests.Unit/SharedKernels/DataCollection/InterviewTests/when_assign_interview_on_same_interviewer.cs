using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_assign_interview_on_same_interviewer : InterviewTestsContext
    {
        private Establish context = () =>
        {
            interviewerId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            supervisorId = Guid.Parse("BBAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Mock.Of<IQuestionnaire>();

            var questionnaireRepository = 
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.AssignInterviewer(supervisorId, interviewerId, DateTime.Now);
        };

        private Because of = () =>
            exception = Catch.Exception(() => interview.AssignInterviewer(supervisorId, interviewerId, DateTime.Now));

        It should_exists_exception = () =>
            exception.ShouldNotBeNull();

        It should_raise_InterviewException = () =>
            exception.ShouldBeOfExactType(typeof(InterviewException));

        It should_contains_exseption_message = () =>
            exception.Message.ShouldNotBeNull();



        private static Guid interviewerId;
        private static Guid supervisorId;
        private static Guid questionnaireId;

        private static Exception exception;
        private static Interview interview;
    }
}
