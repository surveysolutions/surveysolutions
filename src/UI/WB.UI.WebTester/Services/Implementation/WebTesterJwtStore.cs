using System;
using System.Collections.Concurrent;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterJwtStore : IWebTesterJwtStore
    {
        private readonly ConcurrentDictionary<Guid, string> store = new();

        public void StoreToken(Guid interviewId, string jwt)
        {
            store[interviewId] = jwt;
        }

        public string? GetToken(Guid interviewId)
        {
            return store.TryGetValue(interviewId, out var jwt) ? jwt : null;
        }

        public void Remove(Guid interviewId)
        {
            store.TryRemove(interviewId, out _);
        }
    }
}
