using System.Collections.Generic;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IDenormalizerRegistry
    {
        IReadOnlyCollection<BaseDenormalizer> GetDenormalizers(CommittedEvent @event);
    }
}
