using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WB.UI.Designer.Services
{
    public class WebTesterService : IWebTesterService
    {
        private readonly IMemoryCache memoryCache;
        private readonly IOptions<WebTesterSettings> settings;

        public WebTesterService(IMemoryCache memoryCache, IOptions<WebTesterSettings> settings)
        {
            this.memoryCache = memoryCache;
            this.settings = settings;
        }

        string cacheKey(string token) => $"wbtester:{token}";

        public Guid? GetQuestionnaire(string token)
        {
            var questionnaire = memoryCache.Get(cacheKey(token)) as string;
            if (Guid.TryParse(questionnaire, out var result)) return result;

            return null;
        }

        private void AddToCache(string token, Guid questionnaireId)
        {
            memoryCache.Set(cacheKey(token), questionnaireId.ToString(), new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(settings.Value.ExpirationAmountMinutes)
            });
        }

        public string CreateTestQuestionnaire(Guid questionnaireId)
        {
            string token = Guid.NewGuid().ToString();

            AddToCache(token, questionnaireId);

            return token;
        }
    }
}
