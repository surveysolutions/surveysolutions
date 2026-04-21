using System;
using System.Collections.Concurrent;

namespace WB.UI.WebTester.Services.Implementation
{
    public class InMemoryUserContextStore : IUserContextStore
    {
        private readonly ConcurrentDictionary<Guid, RequestUserContext> store = new();

        public void Store(Guid questionnaireId, RequestUserContext context)
            => store[questionnaireId] = context;

        public RequestUserContext? Get(Guid questionnaireId)
        {
            store.TryGetValue(questionnaireId, out var ctx);
            return ctx;
        }

        public void Remove(Guid questionnaireId)
            => store.TryRemove(questionnaireId, out _);
    }
}
