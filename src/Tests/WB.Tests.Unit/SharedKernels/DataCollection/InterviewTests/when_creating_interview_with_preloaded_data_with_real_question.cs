using System;
using System.Collections.Generic;
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
    internal class when_creating_interview_with_preloaded_data_with_real_question : InterviewTestsContext
    {
        private Establish context = () =>
        {
            interviewId = Guid.Parse("11111111111111111111111111111111");
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var fixedRosterGroup = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCD");
            prefilledQuestionAnswer = 2;
            preloadedDataDto = new PreloadedDataDto("id",
                new[]
                {
                    new PreloadedLevelDto(new decimal[0], new Dictionary<Guid, object> { { prefilledQuestionId, prefilledQuestionAnswer } }),
                });
            answersTime = new DateTime(2013, 09, 01);

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.GetQuestionType(prefilledQuestionId) == QuestionType.Numeric
                    && _.HasQuestion(prefilledQuestionId) == true
                    && _.GetFixedRosterGroups(null) == new Guid[] { fixedRosterGroup }
                    && _.GetFixedRosterTitles(fixedRosterGroup) == new FixedRosterTitle[0]);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, 1) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);


            SetupInstanceToMockedServiceLocator<IInterviewExpressionStatePrototypeProvider>(
                CreateInterviewExpressionStateProviderStub());

            eventContext = new EventContext();
        };

        private Because of = () =>
            new Interview(interviewId, userId, questionnaireId, 1, preloadedDataDto, answersTime, supervisorId);

        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private It should_raise_InterviewFromPreloadedDataCreated_event = () =>
            eventContext.ShouldContainEvent<InterviewFromPreloadedDataCreated>();

        private It should_raise_valid_TextQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<NumericRealQuestionAnswered>(@event
                => @event.Answer == prefilledQuestionAnswer && @event.QuestionId == prefilledQuestionId);


        private static EventContext eventContext;
        private static Guid interviewId;
        private static Guid userId;
        private static Guid questionnaireId;
        private static PreloadedDataDto preloadedDataDto;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid prefilledQuestionId;
        private static decimal prefilledQuestionAnswer;
    }
}