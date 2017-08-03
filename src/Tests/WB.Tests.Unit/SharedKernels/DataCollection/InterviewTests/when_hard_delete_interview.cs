using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_hard_delete_interview : InterviewTestsContext
    {
        private Establish context = () =>
        {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            questionnaireId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Mock.Of<IQuestionnaire>();

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () =>
                interview.HardDelete(userId);

        It should_raise_one_events = () =>
            eventContext.Events.Count().ShouldEqual(1);

        It should_raise_InterviewHardDeleted_event = () =>
           eventContext.ShouldContainEvent<InterviewHardDeleted>(@event => @event.UserId == userId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static Guid userId;

        private static Guid questionnaireId;

        private static EventContext eventContext;
        private static Interview interview;
    }
}
