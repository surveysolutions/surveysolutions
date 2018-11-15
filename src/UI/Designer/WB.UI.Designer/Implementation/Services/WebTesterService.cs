using System;
using System.Runtime.Caching;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Implementation.Services
{
    class WebTesterService : IWebTesterService
    {
        private readonly CacheItemPolicy cacheItemPolicy;

        public WebTesterService(WebTesterSettings settings)
        {
            cacheItemPolicy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(settings.ExpirationAmountMinutes)
            };
        }

        private MemoryCache Cache { get; } = new MemoryCache("WebTester");

        string cacheKey(string token) => $"token:{token}";

        public Guid? GetQuestionnaire(string token)
        {
            var questionnaire = Cache.Get(cacheKey(token)) as string;
            if (Guid.TryParse(questionnaire, out var result)) return result;

            return null;
        }

        private void AddToCache(string token, Guid questionnaireId)
        {
            Cache.Add(cacheKey(token), questionnaireId.ToString(), cacheItemPolicy);
        }

        public string CreateTestQuestionnaire(Guid questionnaireId)
        {
            string token = Guid.NewGuid().ToString();

            AddToCache(token, questionnaireId);

            return token;
        }
    }
}
