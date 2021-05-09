using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.InterviewDataStorage.Services;
using WB.Services.Export.Questionnaire.Services.Implementation;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.Questionnaire
{
    public class QuestionnaireStorageCachingTests
    {
        private Mock<IHeadquartersApi> apiMock;
        private TenantContext tenantContext;
        private QuestionnaireStorageCache cache;
        private QuestionnaireStorage storage;
        private MemoryCache memoryCache;
        private TenantInfo tenantInfo;

        [SetUp]
        public void Setup()
        {
            this.apiMock = new Mock<IHeadquartersApi>();

            this.tenantInfo = new TenantInfo("http://example", "hello", "some name");
            this.tenantContext = new TenantContext(Mock.Of<ITenantApi<IHeadquartersApi>>(
                m => m.For(It.IsAny<TenantInfo>()) == this.apiMock.Object), tenantInfo);
            
            this.memoryCache = new MemoryCache(new MemoryCacheOptions());
            this.cache = new QuestionnaireStorageCache(memoryCache, tenantContext);
            this.storage = new QuestionnaireStorage(cache, tenantContext, new NullLogger<QuestionnaireStorage>());
        }

        [Test]
        public async Task should_not_query_questionnaire_from_api_if_present_in_cache_and_db()
        {
            // arrange
            var qId = new QuestionnaireIdentity(Guid.NewGuid() + "$1");
            var doc = Create.QuestionnaireDocument();

            this.apiMock.Setup(a => a.GetQuestionnaireAsync(qId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(JsonConvert.SerializeObject(doc)));

            var token = new CancellationToken();
            await this.storage.GetQuestionnaireAsync(qId, token: token);
            await this.storage.GetQuestionnaireAsync(qId, token: token);
            await this.storage.GetQuestionnaireAsync(qId, token: token);
            await this.storage.GetQuestionnaireAsync(qId, token: token);
            await this.storage.GetQuestionnaireAsync(qId, token: token);
            await this.storage.GetQuestionnaireAsync(qId, token: token);

            this.apiMock.Verify(a => a.GetQuestionnaireAsync(qId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once,
                "should query questionnaire from API on cold cache only once");
        }
    }
}
