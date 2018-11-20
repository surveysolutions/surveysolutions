using System;
using System.Runtime.Caching;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.Designer.Api.WebTester
{
    public class QuestionnaireChacheStorage
    {
        readonly MemoryCache Cache = new MemoryCache("CompilationPackages");

        public Lazy<Questionnaire> Get(string cacheKey)
        {
            return Cache.Get(cacheKey) as Lazy<Questionnaire>;
        }

        public void Add(string cacheKey, Lazy<Questionnaire> cacheEntry)
        {
            Cache.Add(cacheKey, cacheEntry, new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(10)
            });
        }

        public void Remove(string cacheKey)
        {
            Cache.Remove(cacheKey);
        }
    }
}
