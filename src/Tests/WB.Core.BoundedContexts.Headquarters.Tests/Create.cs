using System;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Tests
{
    internal static class Create
    {
        public static InterviewsFeedDenormalizer InterviewsFeedDenormalizer(IReadSideRepositoryWriter<InterviewFeedEntry> feedEntryWriter = null, 
            IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewsRepository = null)
        {
            return new InterviewsFeedDenormalizer(feedEntryWriter ?? Substitute.For<IReadSideRepositoryWriter<InterviewFeedEntry>>(),
                interviewsRepository ?? Substitute.For<IReadSideRepositoryWriter<ViewWithSequence<InterviewData>>>());
        }

        public static QuestionnaireFeedDenormalizer QuestionnaireFeedDenormalizer(IReadSideRepositoryWriter<QuestionnaireFeedEntry> questionnaireFeedWriter)
        {
            return new QuestionnaireFeedDenormalizer(questionnaireFeedWriter);
        }

        public static IPublishedEvent<T> PublishedEvent<T>(Guid eventSourceId, T evnt)
        {
            IPublishedEvent<T> e = new PublishedEvent<T>(new UncommittedEvent(Guid.NewGuid(),
                                                                              eventSourceId,
                                                                              1,
                                                                              1,
                                                                              DateTime.Now,
                                                                              evnt,
                                                                              new Version(1, 0))
                );
            return e;
        }
    }
}