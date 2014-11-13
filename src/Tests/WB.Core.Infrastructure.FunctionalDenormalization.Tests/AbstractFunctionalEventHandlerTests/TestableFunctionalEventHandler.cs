using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.AbstractFunctionalEventHandlerTests
{
    internal class TestableFunctionalEventHandler : AbstractFunctionalEventHandler<IReadSideRepositoryEntity>, IUpdateHandler<IReadSideRepositoryEntity, object>, ICreateHandler<IReadSideRepositoryEntity,string>
    {
        public TestableFunctionalEventHandler(IReadSideRepositoryWriter<IReadSideRepositoryEntity> readsideRepositoryWriter)
            : base(readsideRepositoryWriter) {}

        public int CountOfUpdates { get; private set; }
        public int CountOfCreate { get; private set; }

        public IReadSideRepositoryEntity Update(IReadSideRepositoryEntity currentState, IPublishedEvent<object> evnt)
        {
            CountOfUpdates++;
            return currentState;
        }

        public IReadSideRepositoryEntity Create(IPublishedEvent<string> evnt)
        {
            CountOfCreate++;
            return Mock.Of<IReadSideRepositoryEntity>();
        }
    }
}
