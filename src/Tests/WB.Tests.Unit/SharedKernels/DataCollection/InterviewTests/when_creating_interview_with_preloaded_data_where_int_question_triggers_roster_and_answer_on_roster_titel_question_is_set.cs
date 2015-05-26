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
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_preloaded_data_where_int_question_triggers_roster_and_answer_on_roster_titel_question_is_set : InterviewTestsContext
    {
        private Establish context = () =>
        {
            interviewId = Guid.Parse("11111111111111111111111111111111");
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledIntQuestion = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = StronglyTypedInterviewEvaluator.IdOf.hhMember;
            prefilledIntQuestionAnswer = 1;

            rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            rosterTitleQuestionAnswer = "a";
            preloadedDataDto = new PreloadedDataDto("id",
                new[]
                {
                    new PreloadedLevelDto(new decimal[0],
                        new Dictionary<Guid, object> {{prefilledIntQuestion, prefilledIntQuestionAnswer}}),
                    new PreloadedLevelDto(new decimal[] {0},
                        new Dictionary<Guid, object> {{rosterTitleQuestionId, rosterTitleQuestionAnswer}})
                });

            answersTime = new DateTime(2013, 09, 01);

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.HasQuestion(prefilledIntQuestion) == true
                   && _.GetQuestionType(prefilledIntQuestion) == QuestionType.Numeric
                   && _.IsQuestionInteger(prefilledIntQuestion) == true
                   && _.GetRosterGroupsByRosterSizeQuestion(prefilledIntQuestion) == new Guid[] {rosterGroupId}

                   && _.HasGroup(rosterGroupId) == true
                   && _.GetRosterLevelForGroup(rosterGroupId) == 1
                   && _.GetRostersFromTopToSpecifiedGroup(rosterGroupId) == new Guid[] {rosterGroupId}

                   && _.HasQuestion(rosterTitleQuestionId) == true
                   && _.GetQuestionType(rosterTitleQuestionId) == QuestionType.Text
                   && _.GetRostersFromTopToSpecifiedQuestion(rosterTitleQuestionId) == new Guid[] {rosterGroupId}
                   && _.DoesQuestionSpecifyRosterTitle(rosterTitleQuestionId) == true
                   && _.GetRostersAffectedByRosterTitleQuestion(rosterTitleQuestionId) == new Guid[] { rosterGroupId });

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, 1) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);


            SetupInstanceToMockedServiceLocator<IInterviewExpressionStatePrototypeProvider>(
                CreateInterviewExpressionStateProviderStub());

            eventContext = new EventContext();
        };

        Because of = () =>
            new Interview(interviewId, userId, questionnaireId, 1, preloadedDataDto, answersTime, supervisorId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_InterviewFromPreloadedDataCreated_event = () =>
            eventContext.ShouldContainEvent<InterviewFromPreloadedDataCreated>();

        It should_raise_valid_TextQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>(@event
                => @event.Answer == prefilledIntQuestionAnswer && @event.QuestionId == prefilledIntQuestion);

        It should_raise_RosterInstancesTitleChanged_event = () =>
           eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
               => @event.ChangedInstances[0].Title == rosterTitleQuestionAnswer && @event.ChangedInstances[0].RosterInstance.GroupId == rosterGroupId);


        private static EventContext eventContext;
        private static Guid interviewId;
        private static Guid userId;
        private static Guid questionnaireId;
        private static PreloadedDataDto preloadedDataDto;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid rosterGroupId;
        private static Guid prefilledIntQuestion;
        private static int prefilledIntQuestionAnswer;
        private static Guid rosterTitleQuestionId;
        private static string rosterTitleQuestionAnswer;
    }
}
