using System;
using System.Web.Caching;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Implementation.Services
{
    class WebTesterService : IWebTesterService
    {
        private readonly WebTesterSettings settings;

        public WebTesterService(WebTesterSettings settings)
        {
            this.settings = settings;
        }

        private Cache Cache => System.Web.HttpRuntime.Cache;
        
        string cacheKey(string token) => $"webtester:token:{token}";

        public Guid? GetQuestionnaire(string token)
        {
            var questionnaire = Cache.Get(cacheKey(token)) as string;
            if (Guid.TryParse(questionnaire, out var result)) return result;

            return null;
        }

        private void AddToCache(string token, Guid questionnaireId)
        {
            Cache.Add(cacheKey(token), questionnaireId.ToString(), null,
                Cache.NoAbsoluteExpiration,
                TimeSpan.FromMinutes(this.settings.ExpirationAmountMinutes), CacheItemPriority.Normal, null);
        }

        public string CreateTestQuestionnaire(Guid questionnaireId)
        {
            string token = Guid.NewGuid().ToString();

            AddToCache(token, questionnaireId);

            return token;
        }
    }
}