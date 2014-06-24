using System;
using System.Net.Http;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NSubstitute;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Tests
{
    internal static class Create
    {
        public static HeadquartersLoginService HeadquartersLoginService(IHeadquartersUserReader headquartersUserReader = null,
            Func<HttpMessageHandler> messageHandler = null,
            ILogger logger = null,
            ICommandService commandService = null,
            HeadquartersSettings headquartersSettings = null)
        {
            return new HeadquartersLoginService(logger ?? Substitute.For<ILogger>(),
                commandService ?? Substitute.For<ICommandService>(),
                messageHandler ?? Substitute.For<Func<HttpMessageHandler>>(),
                headquartersSettings ?? HeadquartersSettings(),
                headquartersUserReader ?? Substitute.For<IHeadquartersUserReader>());
        }

        public static UserChangedFeedReader UserChangedFeedReader(HeadquartersSettings settings = null,
            Func<HttpMessageHandler> messageHandler = null)
        {
            return new UserChangedFeedReader(settings ?? HeadquartersSettings(),
                messageHandler ?? Substitute.For<Func<HttpMessageHandler>>(), HeadquartersPullContext());
        }

        public static HeadquartersPullContext HeadquartersPullContext()
        {
            return new HeadquartersPullContext(Substitute.For<IPlainStorageAccessor<SynchronizationStatus>>());
        }

        public static HeadquartersPushContext HeadquartersPushContext()
        {
            return new HeadquartersPushContext(Substitute.For<IPlainStorageAccessor<SynchronizationStatus>>());
        }

        public static InterviewsSynchronizer InterviewsSynchronizer(
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter = null,
            IQueryableReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview> readyToSendInterviewsRepositoryWriter = null,
            Func<HttpMessageHandler> httpMessageHandler = null,
            IEventStore eventStore = null,
            ILogger logger = null,
            IJsonUtils jsonUtils = null,
            ICommandService commandService = null,
            HeadquartersPushContext headquartersPushContext = null)
        {
            return new InterviewsSynchronizer(
                Mock.Of<IAtomFeedReader>(),
                HeadquartersSettings(),
                logger ?? Mock.Of<ILogger>(),
                commandService ?? Mock.Of<ICommandService>(),
                Mock.Of<IQueryablePlainStorageAccessor<LocalInterviewFeedEntry>>(),
                Mock.Of<IQueryableReadSideRepositoryReader<UserDocument>>(),
                Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<IHeadquartersQuestionnaireReader>(),
                Mock.Of<IHeadquartersInterviewReader>(),
                HeadquartersPullContext(),
                headquartersPushContext ?? HeadquartersPushContext(),
                eventStore ?? Mock.Of<IEventStore>(),
                jsonUtils ?? Mock.Of<IJsonUtils>(),
                interviewSummaryRepositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                readyToSendInterviewsRepositoryWriter ?? Mock.Of<IQueryableReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview>>(),
                httpMessageHandler ?? Mock.Of<Func<HttpMessageHandler>>());
        }

        public static HeadquartersSettings HeadquartersSettings(Uri loginServiceUri = null,
            Uri usersChangedFeedUri = null,
            Uri interviewsFeedUri = null,
            string questionnaireDetailsEndpoint = "",
            string accessToken = "",
            Uri interviewsPushUrl = null)
        {
            return new HeadquartersSettings(loginServiceUri ?? new Uri("http://localhost/"),
                usersChangedFeedUri ?? new Uri("http://localhost/"),
                interviewsFeedUri ?? new Uri("http://localhost/"),
                questionnaireDetailsEndpoint,
                accessToken,
                interviewsPushUrl ?? new Uri("http://localhost"));
        }

        public static CommittedEvent CommittedEvent(string origin = null, Guid? eventSourceId = null, object payload = null,
            Guid? eventIdentifier = null, long eventSequence = 1)
        {
            return new CommittedEvent(
                Guid.Parse("33330000333330000003333300003333"),
                origin,
                eventIdentifier ?? Guid.Parse("44440000444440000004444400004444"),
                eventSourceId ?? Guid.Parse("55550000555550000005555500005555"),
                eventSequence,
                new DateTime(2014, 10, 22),
                payload ?? "some payload",
                new Version());
        }

        public static InterviewSummary InterviewSummary()
        {
            return new InterviewSummary();
        }

        public static Synchronizer Synchronizer(IInterviewsSynchronizer interviewsSynchronizer = null)
        {
            return new Synchronizer(
                Mock.Of<ILocalFeedStorage>(),
                Mock.Of<IUserChangedFeedReader>(),
                Mock.Of<ILocalUserFeedProcessor>(),
                interviewsSynchronizer ?? Mock.Of<IInterviewsSynchronizer>(),
                HeadquartersPullContext(),
                HeadquartersPushContext());
        }
    }
}