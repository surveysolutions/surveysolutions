using System;
using Ncqrs.Eventing.Storage;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IEnumeratorEventStorage : IEventStore
    {
        void RemoveEventSourceById(Guid interviewId);
    }
}
