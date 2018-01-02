using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebPages;
using Ncqrs;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Designer.Implementation.Services
{
    class WebTesterService : IWebTesterService
    {
        private readonly IClock clock; 

        public WebTesterService(IClock clock, IConfigurationManager configurationManager)
        {
            this.clock = clock;

            expirationAmount = configurationManager.AppSettings["WebTester.ExpirationAmountMinutes"].AsInt(10);

            Task.Factory.StartNew(async () =>
            {
                Thread.CurrentThread.Name = "Questionnaire test cleanup task";
                while (true)
                {
                    var items = questionnaireMap.Keys.ToList();

                    foreach (var item in items)
                    {
                        if(questionnaireMap.TryGetValue(item, out var cacheItem))
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

                    await Task.Delay(TimeSpan.FromSeconds(10));
                }

            }, TaskCreationOptions.LongRunning);
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
        private readonly int expirationAmount;

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

        private DateTime GetExpirationTime() => clock.UtcNow().AddMinutes(expirationAmount);

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