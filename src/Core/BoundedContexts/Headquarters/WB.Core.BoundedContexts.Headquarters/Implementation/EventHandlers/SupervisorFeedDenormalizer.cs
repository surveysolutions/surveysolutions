using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.SupervisorFeed;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers
{
    public class SupervisorFeedDenormalizer : BaseDenormalizer, IEventHandler<SupervisorRegistered>
    {
        private readonly IReadSideRepositoryWriter<SupervisorRegisteredEntry> writer;

        public SupervisorFeedDenormalizer(IReadSideRepositoryWriter<SupervisorRegisteredEntry> writer)
        {
            this.writer = writer;
        }

        public override Type[] BuildsViews
        {
            get { return new Type[] { typeof (SupervisorFeedDenormalizer) }; }
        }

        public void Handle(IPublishedEvent<SupervisorRegistered> evnt)
        {
            writer.Store(new SupervisorRegisteredEntry
            {
                Login = evnt.Payload.Login,
                PasswordHash = evnt.Payload.PasswordHash
            }, evnt.Payload.Login);
        }
    }
}