using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.AbstractFunctionalEventHandlerTests
{
    internal class TestableFunctionalEventHandler<T> : AbstractFunctionalEventHandler<T>, IUpdateHandler<T,object> where T : class, IReadSideRepositoryEntity
    {
        public TestableFunctionalEventHandler(IReadSideRepositoryWriter<T> readsideRepositoryWriter)
            : base(readsideRepositoryWriter) {}

        public T Update(T currentState, IPublishedEvent<object> evnt)
        {
            CountOfUpdates++;
            return currentState;
        }

        public int CountOfUpdates { get; private set; }
    }
}
