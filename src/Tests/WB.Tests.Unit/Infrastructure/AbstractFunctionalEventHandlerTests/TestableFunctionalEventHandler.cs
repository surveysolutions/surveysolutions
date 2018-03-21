using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class TestableFunctionalEvent : IEvent { }

    internal class TestableFunctionalEventHandler : AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideRepositoryWriter<IReadSideRepositoryEntity>>, IUpdateHandler<IReadSideRepositoryEntity, TestableFunctionalEvent>
    {
        public TestableFunctionalEventHandler(IReadSideRepositoryWriter<IReadSideRepositoryEntity> readSideStorage)
            : base(readSideStorage) {}

        public int CountOfUpdates { get; private set; }

        public IReadSideRepositoryEntity Update(IReadSideRepositoryEntity state, IPublishedEvent<TestableFunctionalEvent> @event)
        {
            this.CountOfUpdates++;
            return state;
        }
    }
}
