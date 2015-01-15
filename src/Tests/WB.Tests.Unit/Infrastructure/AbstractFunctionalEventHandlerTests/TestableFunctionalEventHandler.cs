using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class TestableFunctionalEventHandler : AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideRepositoryWriter<IReadSideRepositoryEntity>>, IUpdateHandler<IReadSideRepositoryEntity, object>, ICreateHandler<IReadSideRepositoryEntity, string>
    {
        public TestableFunctionalEventHandler(IReadSideRepositoryWriter<IReadSideRepositoryEntity> readSideStorage)
            : base(readSideStorage) {}

        public int CountOfUpdates { get; private set; }
        public int CountOfCreate { get; private set; }

        public IReadSideRepositoryEntity Update(IReadSideRepositoryEntity currentState, IPublishedEvent<object> evnt)
        {
            this.CountOfUpdates++;
            return currentState;
        }

        public IReadSideRepositoryEntity Create(IPublishedEvent<string> evnt)
        {
            this.CountOfCreate++;
            return Mock.Of<IReadSideRepositoryEntity>();
        }
    }
}
