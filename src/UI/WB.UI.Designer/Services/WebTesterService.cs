using System;
using System.Runtime.Caching;
using Microsoft.Extensions.Options;
using WB.UI.Designer.Implementation.Services;

namespace WB.UI.Designer.Services
{
    public class WebTesterService : IWebTesterService
    {
        private readonly IOptions<WebTesterSettings> settings;

        public WebTesterService(IOptions<WebTesterSettings> settings)
        {
            this.settings = settings;
            
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
            var cacheItemPolicy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(settings.Value.ExpirationAmountMinutes)
            };
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
