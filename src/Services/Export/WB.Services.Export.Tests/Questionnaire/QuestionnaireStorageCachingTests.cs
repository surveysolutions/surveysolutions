using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.InterviewDataStorage.Services;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services.Implementation;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.Questionnaire
{
    public class QuestionnaireStorageCachingTests
    {
        private Mock<IHeadquartersApi> apiMock;
        private TenantContext tenantContext;
        private QuestionnaireStorageCache cache;
        private TenantDbContext db;
        private QuestionnaireStorage storage;
        private MemoryCache memoryCache;
        private DatabaseSchemaService dbSchema;
        private TenantInfo tenantInfo;

        [SetUp]
        public void Setup()
        {
            this.apiMock = new Mock<IHeadquartersApi>();

            this.tenantInfo = new TenantInfo("http://example", "hello", "some name");
            this.tenantContext = new TenantContext(Mock.Of<ITenantApi<IHeadquartersApi>>(
                m => m.For(It.IsAny<TenantInfo>()) == this.apiMock.Object));

            this.tenantContext.Tenant = tenantInfo;
            this.memoryCache = new MemoryCache(new MemoryCacheOptions());

            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString("N"))
                .Options;
            this.db = new TenantDbContext(tenantContext, Options.Create(new DbConnectionSettings()), options);

            this.cache = new QuestionnaireStorageCache(db, memoryCache, tenantContext);

            this.storage = new QuestionnaireStorage(cache, tenantContext, new NullLogger<QuestionnaireStorage>());
        }

        [Test]
        public async Task should_not_query_questionnaire_from_api_if_present_in_cache_and_db()
        {
            // arrange
            var qId = new QuestionnaireId(Guid.NewGuid() + "$1");
            var doc = Create.QuestionnaireDocument();

            this.apiMock.Setup(a => a.GetQuestionnaireAsync(qId))
                .Returns(Task.FromResult(doc));

            await this.storage.GetQuestionnaireAsync(qId);
            await this.storage.GetQuestionnaireAsync(qId);
            await this.storage.GetQuestionnaireAsync(qId);
            await this.storage.GetQuestionnaireAsync(qId);
            await this.storage.GetQuestionnaireAsync(qId);
            await this.storage.GetQuestionnaireAsync(qId);

            this.apiMock.Verify(a => a.GetQuestionnaireAsync(qId), Times.Once,
                "should query questionnaire from API on cold cache only once");
        }

        [TestCase(true, false, true)]
        [TestCase(false, true, true)]
        [TestCase(false, null, true)]
        [TestCase(false, false, false)]
        [TestCase(true, true, false)]
        public void invalidate_questionnaire_cache_if_deleted_status_differ(
            bool questionnaireInCacheIsDeleted, 
            bool? inDatabaseDeleted, 
            bool isCacheInvalidated)
        {
            var qId = new QuestionnaireId(Guid.NewGuid() + "$1");
            var doc = Create.QuestionnaireDocument();

            this.cache.Set(qId, doc);

            if (questionnaireInCacheIsDeleted) doc.IsDeleted = true;
            if (inDatabaseDeleted != null)
            {
                var reference = new GeneratedQuestionnaireReference(qId);
                if (inDatabaseDeleted == true) reference.MarkAsDeleted();

                this.db.GeneratedQuestionnaires.Add(reference);
            }

            Assert.That(this.cache.TryGetValue(qId, out _), Is.EqualTo(!isCacheInvalidated));
        }
    }
}
