using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers
{
    internal class SupervisorLoginsDenormalizer : BaseDenormalizer,
        IEventHandler<SupervisorRegistered>
    {
        private readonly IReadSideRepositoryWriter<SupervisorLoginView> repositoryWriter;

        public SupervisorLoginsDenormalizer(IReadSideRepositoryWriter<SupervisorLoginView> repositoryWriter)
        {
            this.repositoryWriter = repositoryWriter;
        }

        public override Type[] BuildsViews
        {
            get { return new[] { typeof(SupervisorLoginView) }; }
        }

        public void Handle(IPublishedEvent<SupervisorRegistered> evnt)
        {
            repositoryWriter.Store(new SupervisorLoginView(), evnt.Payload.Login);
        }
    }
}