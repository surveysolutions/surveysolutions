using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Implementation.Services
{
    class WebTesterService : IWebTesterService
    {
        private readonly IClock clock;
        private readonly WebTesterSettings settings;

        public WebTesterService(IClock clock, WebTesterSettings settings)
        {
            this.clock = clock;
            this.settings = settings;
        }

        public void StartBackgroundCleanupJob()
        {
            Task.Factory.StartNew(async () => {
                Thread.CurrentThread.Name = "Questionnaire test cleanup task";
                
                while (true)
                {
                    Cleanup();

                    await Task.Delay(TimeSpan.FromSeconds(10));
                }

            }, TaskCreationOptions.LongRunning);
        }

        public void Cleanup()
        {
            var items = questionnaireMap.Keys.ToList();

            foreach (var item in items)
            {
                if (questionnaireMap.TryGetValue(item, out var cacheItem))
                {
                    if (cacheItem.IsExpired(clock.UtcNow()))
                    {
                        if (questionnaireMap.TryRemove(cacheItem.QuestionnaireId, out var existingItem))
                        {
                            tokenToQuestionnaireMap.TryRemove(existingItem.Token, out _);
                        }
                    }
                }
            }
        }

        private sealed class CacheItem
        {
            public Guid QuestionnaireId { get; set; }
            public string Token { get; set; }
            public DateTime ExpirationTime { private get; set; }

            public bool IsExpired(DateTime now) => now > ExpirationTime;
        }

        private readonly ConcurrentDictionary<Guid, CacheItem> questionnaireMap = new ConcurrentDictionary<Guid, CacheItem>();
        private readonly ConcurrentDictionary<string, CacheItem> tokenToQuestionnaireMap = new ConcurrentDictionary<string, CacheItem>();

        public Guid? GetQuestionnaire(string token)
        {
            if (tokenToQuestionnaireMap.TryGetValue(token, out var cacheItem))
            {
                if (cacheItem.IsExpired(clock.UtcNow()))
                {
                    return null;
                }

                cacheItem.ExpirationTime = GetExpirationTime();
                return cacheItem.QuestionnaireId;
            }

            return null;
        }

        private DateTime GetExpirationTime() => clock.UtcNow().AddMinutes(settings.ExpirationAmountMinutes);

        public string CreateTestQuestionnaire(Guid questionnaireId)
        {
            var cacheItem = new CacheItem
            {
                QuestionnaireId = questionnaireId,
                Token = Guid.NewGuid().ToString(),
                ExpirationTime = GetExpirationTime()
            };

            if (questionnaireMap.TryRemove(questionnaireId, out var existingItem))
            {
                tokenToQuestionnaireMap.TryRemove(existingItem.Token, out _);
            }

            questionnaireMap.AddOrUpdate(questionnaireId, cacheItem, (key, item) => cacheItem);
            tokenToQuestionnaireMap.AddOrUpdate(cacheItem.Token, cacheItem, (key, item) => cacheItem);

            return cacheItem.Token;
        }
    }
}