﻿using System;
using Main.Core.Documents;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.StatisticsDenormalizerTests
{
    internal class StatisticsDenormalizerTestContext
    {
        protected static StatisticsDenormalizer CreateStatisticsDenormalizer(
                        IReadSideRepositoryWriter<StatisticsLineGroupedByUserAndTemplate> statisticsStorage = null,
                        IReadSideRepositoryWriter<UserDocument> users = null,
                        IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage = null,
                        IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires = null)
        {
            return new StatisticsDenormalizer(
                statisticsStorage ?? Mock.Of<IReadSideRepositoryWriter<StatisticsLineGroupedByUserAndTemplate>>(),
                users ?? Mock.Of<IReadSideRepositoryWriter<UserDocument>>(),
                interviewBriefStorage ?? Mock.Of<IReadSideRepositoryWriter<InterviewBrief>>(),
                questionnaires ?? Mock.Of<IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem>>());
        }


        protected static Mock<IReadSideRepositoryWriter<StatisticsLineGroupedByUserAndTemplate>> CreateStatisticsStorage(StatisticsLineGroupedByUserAndTemplate statistics)
        {
            var statisticsStorage = new Mock<IReadSideRepositoryWriter<StatisticsLineGroupedByUserAndTemplate>>();
            statisticsStorage.Setup(x => x.GetById(Moq.It.IsAny<string>())).Returns(statistics);
            
            return statisticsStorage;
        }

        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid eventSourceId)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == eventSourceId);
        }

        protected static IPublishedEvent<InterviewStatusChanged> CreateInterviewStatusChangedEvent(InterviewStatus status, Guid eventSourceId)
        {
            var evnt = ToPublishedEvent(new InterviewStatusChanged(status, String.Empty, new Guid()), eventSourceId);
            return evnt;
        }

    }
}
