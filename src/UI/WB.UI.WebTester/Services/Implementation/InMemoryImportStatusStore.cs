using System;
using System.Collections.Concurrent;

namespace WB.UI.WebTester.Services.Implementation
{
    public class InMemoryImportStatusStore : IImportStatusStore
    {
        private static readonly ConcurrentDictionary<Guid, CreationResult> statuses = new();

        public bool TryInitialize(Guid interviewId)
            => statuses.TryAdd(interviewId, CreationResult.Loading);

        public void Set(Guid interviewId, CreationResult result)
            => statuses[interviewId] = result;

        public CreationResult? Get(Guid interviewId)
            => statuses.TryGetValue(interviewId, out var result) ? result : null;

        public CreationResult? Remove(Guid interviewId)
            => statuses.TryRemove(interviewId, out var result) ? result : null;
    }
}
