using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Tests
{
    internal static class Create
    {
        internal static SurvisorCredentialsDenormalizer SurvisorCredentialsDenormalizer(IReadSideRepositoryWriter<SupervisorCredentialsView> repositoryWriter = null)
        {
            return new SurvisorCredentialsDenormalizer(
                repositoryWriter ?? Substitute.For<IReadSideRepositoryWriter<SupervisorCredentialsView>>()
            );
        }

        internal static IPublishedEvent<T> PublishedEvent<T>(T @event = null, Guid? eventSourceId = null) where T : class
        {
            var publishedEvent = Substitute.For<IPublishedEvent<T>>();
            publishedEvent.Payload.Returns(@event);
            publishedEvent.EventSourceId.Returns(eventSourceId ?? Guid.Parse("1234567890abcdef0101010102020304"));
            publishedEvent.EventSequence.Returns(1);

            return publishedEvent;
        }

        public static SupervisorLoginService SupervisorLoginService(IQueryableReadSideRepositoryReader<SupervisorLoginView> supervisorLogins = null,
            IQueryableReadSideRepositoryReader<SupervisorCredentialsView> credentialsStore = null,
            IPasswordHasher passwordHasher = null)
        {
            return new SupervisorLoginService(supervisorLogins ?? Substitute.For<IQueryableReadSideRepositoryReader<SupervisorLoginView>>(),
                credentialsStore ?? Substitute.For<IQueryableReadSideRepositoryReader<SupervisorCredentialsView>>(),
                passwordHasher ?? Substitute.For<IPasswordHasher>());
        }
    }
}