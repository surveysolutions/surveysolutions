using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers
{
    internal class SurvisorCredentialsDenormalizer : IEventHandler, IEventHandler<SupervisorRegistered>
    {
        private readonly IReadSideRepositoryWriter<SupervisorCredentialsView> repositoryWriter;

        public SurvisorCredentialsDenormalizer(IReadSideRepositoryWriter<SupervisorCredentialsView> repositoryWriter)
        {
            this.repositoryWriter = repositoryWriter;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new[] { typeof(SupervisorCredentialsView) }; }
        }

        public void Handle(IPublishedEvent<SupervisorRegistered> evnt)
        {
            this.repositoryWriter.Store(new SupervisorCredentialsView(), string.Join(":", evnt.Payload.Login, evnt.Payload.PasswordHash));
        }
    }
}