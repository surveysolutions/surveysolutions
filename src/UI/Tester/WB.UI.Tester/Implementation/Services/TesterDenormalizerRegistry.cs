using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Tester.Implementation.Services
{
    public class TesterDenormalizerRegistry : IDenormalizerRegistry
    {
        public IReadOnlyCollection<BaseDenormalizer> GetDenormalizers(CommittedEvent @event) => Array.Empty<BaseDenormalizer>();
    }
}
