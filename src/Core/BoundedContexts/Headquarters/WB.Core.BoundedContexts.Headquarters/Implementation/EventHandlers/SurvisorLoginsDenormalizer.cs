using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers
{
    internal class SurvisorLoginsDenormalizer : IEventHandler,
        IEventHandler<SupervisorRegistered>
    {
        private readonly IReadSideRepositoryWriter<SupervisorLoginView> repositoryWriter;

        public SurvisorLoginsDenormalizer(IReadSideRepositoryWriter<SupervisorLoginView> repositoryWriter)
        {
            this.repositoryWriter = repositoryWriter;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new[] { typeof(SupervisorLoginView) }; }
        }

        public void Handle(IPublishedEvent<SupervisorRegistered> evnt)
        {
            repositoryWriter.Store(new SupervisorLoginView(), evnt.Payload.Login);
        }
    }
}