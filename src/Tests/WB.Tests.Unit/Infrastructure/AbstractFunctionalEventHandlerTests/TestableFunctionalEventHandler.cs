﻿using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class TestableFunctionalEventHandler : AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideRepositoryWriter<IReadSideRepositoryEntity>>, IUpdateHandler<IReadSideRepositoryEntity, ILiteEvent>
    {
        public TestableFunctionalEventHandler(IReadSideRepositoryWriter<IReadSideRepositoryEntity> readSideStorage)
            : base(readSideStorage) {}

        public int CountOfUpdates { get; private set; }

        public IReadSideRepositoryEntity Update(IReadSideRepositoryEntity currentState, IPublishedEvent<ILiteEvent> evnt)
        {
            this.CountOfUpdates++;
            return currentState;
        }
    }
}
