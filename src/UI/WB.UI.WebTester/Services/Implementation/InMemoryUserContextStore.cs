using System;
using System.Collections.Concurrent;

namespace WB.UI.WebTester.Services.Implementation
{
    public class InMemoryUserContextStore : IUserContextStore
    {
        private readonly ConcurrentDictionary<Guid, RequestUserContext> store = new();

        public void Store(Guid interviewId, RequestUserContext context)
            => store[interviewId] = context;

        public RequestUserContext? Get(Guid interviewId)
        {
            store.TryGetValue(interviewId, out var ctx);
            return ctx;
        }

        public void Remove(Guid interviewId)
            => store.TryRemove(interviewId, out _);
    }
}
