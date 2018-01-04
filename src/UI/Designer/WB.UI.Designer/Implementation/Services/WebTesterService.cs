using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, Guid> localCache = new ConcurrentDictionary<string, Guid>();
        
        string questionnaireKey(Guid id) => $"___questionnaire_key___{id.ToString()}";

        public Guid? GetQuestionnaire(string token)
        {
            if(localCache.TryGetValue(token, out var questionnaireId))
            {
                if(Cache.Get(questionnaireKey(questionnaireId)) as string == token)
                    return questionnaireId;
            }

            return null;
        }

        private void AddToCache(string token, Guid questionnaireId)
        {
            localCache.AddOrUpdate(token, questionnaireId,(k, q) => questionnaireId);

            Cache.Add(questionnaireKey(questionnaireId), token, null,
                Cache.NoAbsoluteExpiration,
                TimeSpan.FromMinutes(this.settings.ExpirationAmountMinutes), CacheItemPriority.Normal,
                (key, item, reason) => { localCache.TryRemove(item.ToString(), out _); });
        }

        public string CreateTestQuestionnaire(Guid questionnaireId)
        {
            Cache.Remove(questionnaireKey(questionnaireId));
            
            string token = Guid.NewGuid().ToString();

            AddToCache(token, questionnaireId);

            return token;
        }
    }
}