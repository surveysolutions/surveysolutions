using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    //[Ignore("http://issues.wbcapi.org/youtrack/issue/KP-5249")]
    internal class when_answer_on_multy_option_question_increases_roster_size_and_disable_rows_at_the_same_time : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            numericQuestionInsideRoster = Guid.Parse("33333333333333333333333333333333");
            rosterGroupId = Guid.Parse("11111111111111111111111111111111");

            multyOptionRosterSizeId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.HasQuestion(multyOptionRosterSizeId) == true
                && _.GetQuestionType(multyOptionRosterSizeId) == QuestionType.MultyOption
                && _.GetRosterGroupsByRosterSizeQuestion(multyOptionRosterSizeId) == new Guid[] { rosterGroupId }
                && _.GetAnswerOptionsAsValues(multyOptionRosterSizeId) == new decimal[] { 1, 2, 3 }

                && _.HasGroup(rosterGroupId) == true
                && _.GetRosterLevelForGroup(rosterGroupId) == 1
                && _.GetRostersFromTopToSpecifiedGroup(rosterGroupId) == new Guid[] { rosterGroupId }

                && _.GetQuestionType(numericQuestionInsideRoster) == QuestionType.Numeric
                && _.IsQuestionInteger(numericQuestionInsideRoster) == true
                && _.HasQuestion(numericQuestionInsideRoster)==true
                && _.GetRostersFromTopToSpecifiedQuestion(numericQuestionInsideRoster) == new Guid[] { rosterGroupId });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            var enablementQueue = new Queue<EnablementChanges>();
            // init .ctor call
            enablementQueue.Enqueue(new EnablementChanges(new List<Identity>(), new List<Identity>(), new List<Identity>(), new List<Identity>()));
            // add first row to roster
            enablementQueue.Enqueue(new EnablementChanges(new List<Identity>(), new List<Identity>(), new List<Identity>(), new List<Identity>()));
            // add second row to roster with disable
            enablementQueue.Enqueue(
                new EnablementChanges(
                    new List<Identity>
                    {
                        new Identity(rosterGroupId, new decimal[] {1}),
                        new Identity(rosterGroupId, new decimal[] {2})
                    }, new List<Identity>(),
                    new List<Identity>() {
                        new Identity(numericQuestionInsideRoster, new decimal[] {1}),
                        new Identity(numericQuestionInsideRoster, new decimal[] {2})
                    },
                    new List<Identity>()));
            //remove first row
            enablementQueue.Enqueue(new EnablementChanges(new List<Identity>(), new List<Identity>(), new List<Identity>(), new List<Identity>()));
            //remove second row
            enablementQueue.Enqueue(new EnablementChanges(new List<Identity>(), new List<Identity>(), new List<Identity>(), new List<Identity>()));
            //return first row
            enablementQueue.Enqueue(new EnablementChanges(new List<Identity>(), new List<Identity>(), new List<Identity>(), new List<Identity>(){
                      // if uncomment this line the test  become succesefull                                                                                                                         
                        new Identity(numericQuestionInsideRoster, new decimal[] {1}),
                        new Identity(numericQuestionInsideRoster, new decimal[] {2})
                    }));
            //answer on numeric question
            enablementQueue.Enqueue(new EnablementChanges(new List<Identity>(), new List<Identity>(), new List<Identity>(), new List<Identity>()));

            interviewExpressionState = new Mock<IInterviewExpressionState>();
            interviewExpressionState.Setup(x => x.ProcessEnablementConditions()).Returns(enablementQueue.Dequeue);
            interviewExpressionState.Setup(x => x.Clone()).Returns(interviewExpressionState.Object);

            SetupInstanceToMockedServiceLocator(
                Mock.Of<IInterviewExpressionStatePrototypeProvider>(
                    x =>
                        x.GetExpressionState(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) ==
                        interviewExpressionState.Object));

            interview = CreateInterview(questionnaireId: questionnaireId);

            interview.SynchronizeInterview(
                userId,
                new InterviewSynchronizationDto(interview.EventSourceId, InterviewStatus.InterviewerAssigned, null, userId, questionnaireId,
                    questionnaire.Version,
                    new AnsweredQuestionSynchronizationDto[0],
                    new HashSet<InterviewItemId>(),
                    new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), null,
                    new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(),
                    true));

            interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 1 });
            interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 1, 2 });
            interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 2 });
            interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[0]);
            interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 1});

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, numericQuestionInsideRoster, new decimal[]{1}, DateTime.Now, 18);

        It should_raise_NumericIntegerQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>(@event
                => @event.Answer==18 && @event.QuestionId==numericQuestionInsideRoster);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid multyOptionRosterSizeId;
        private static Guid numericQuestionInsideRoster;
        private static Guid rosterGroupId;
        private static Mock<IInterviewExpressionState> interviewExpressionState;
    }
}
