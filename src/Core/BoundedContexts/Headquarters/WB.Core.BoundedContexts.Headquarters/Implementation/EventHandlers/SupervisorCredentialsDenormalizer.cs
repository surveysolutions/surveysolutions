using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers
{
    internal class SupervisorCredentialsDenormalizer : BaseDenormalizer, IEventHandler<SupervisorRegistered>
    {
        private readonly IReadSideRepositoryWriter<SupervisorCredentialsView> repositoryWriter;

        public SupervisorCredentialsDenormalizer(IReadSideRepositoryWriter<SupervisorCredentialsView> repositoryWriter)
        {
            this.repositoryWriter = repositoryWriter;
        }

        public override Type[] BuildsViews
        {
            get { return new[] { typeof(SupervisorCredentialsView) }; }
        }

        public void Handle(IPublishedEvent<SupervisorRegistered> evnt)
        {
            this.repositoryWriter.Store(new SupervisorCredentialsView(), string.Join(":", evnt.Payload.Login, evnt.Payload.PasswordHash));
        }
    }
}