using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IEventWithAffectedEntities
    {
        IReadOnlyCollection<Identity> GetAffectedEntities();
    }
}
