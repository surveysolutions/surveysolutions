#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage;
using WB.UI.Designer.Filters;

namespace WB.Tests.Integration.Designer
{
    // Exercises TransactionFilter over a REAL PostgreSQL DesignerDbContext. The handler flushes SaveChanges
    // inside the request (as production handlers do), so these tests actually prove relational rollback works,
    // unlike the InMemory unit tests where BeginTransaction/Rollback are no-ops. Every assertion reads from a
    // FRESH context/connection so it observes only committed state.
    [TestOf(typeof(TransactionFilter))]
    [NonParallelizable]
    internal class TransactionFilterIntegrationTests : IntegrationTest
    {
        [Test]
        public void when_write_handler_flushes_changes_then_throws_row_is_rolled_back()
        {
            var dbContext = ServiceLocator.GetInstance<DesignerDbContext>();
            var id = Guid.NewGuid();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                RunThroughActionFilter(dbContext, HttpMethods.Post, throwInHandler: true,
                    handlerBody: () => StageAndFlush(dbContext, id)));

            Assert.That(RowExists(dbContext, id), Is.False);
        }

        [Test]
        public void when_write_handler_throws_cache_invalidation_runs_after_transaction_disposal()
        {
            var dbContext = ServiceLocator.GetInstance<DesignerDbContext>();
            var id = Guid.NewGuid();
            var invalidation = new AssertTransactionSettledInvalidation(dbContext, () => RowExists(dbContext, id));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                RunThroughActionFilter(dbContext, HttpMethods.Post, throwInHandler: true,
                    handlerBody: () => StageAndFlush(dbContext, id), invalidation: invalidation));

            Assert.That(invalidation.FlushCalled, Is.True);
        }

        [Test]
        public async Task when_write_handler_flushes_changes_and_succeeds_row_is_committed()
        {
            var dbContext = ServiceLocator.GetInstance<DesignerDbContext>();
            var id = Guid.NewGuid();

            await RunThroughActionFilter(dbContext, HttpMethods.Post, throwInHandler: false,
                handlerBody: () => StageAndFlush(dbContext, id));

            Assert.That(RowExists(dbContext, id), Is.True);
        }

        [Test]
        public async Task when_safe_handler_flushes_changes_they_are_rolled_back()
        {
            var dbContext = ServiceLocator.GetInstance<DesignerDbContext>();
            var id = Guid.NewGuid();

            await RunThroughActionFilter(dbContext, HttpMethods.Get, throwInHandler: false,
                handlerBody: () => StageAndFlush(dbContext, id));

            Assert.That(RowExists(dbContext, id), Is.False);
        }

        [Test]
        public async Task when_safe_request_uses_transaction_filter_questionnaire_reads_can_still_use_shared_cache()
        {
            var dbContext = ServiceLocator.GetInstance<DesignerDbContext>();
            var questionnaireId = Guid.NewGuid();
            SeedQuestionnaireDocument(dbContext, questionnaireId, "database");

            var serializer = new EntitySerializer<QuestionnaireDocument>();
            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var evictionTokens = new KeyValueCacheEvictionTokens();
            var invalidation = new TransactionalMemoryCacheInvalidation(memoryCache, evictionTokens);
            var storage = new DesignerKeyValueStorage<QuestionnaireDocument>(dbContext, memoryCache, serializer, invalidation, evictionTokens);
            var cacheKey = nameof(StoredQuestionnaireDocument) + questionnaireId.FormatGuid();
            memoryCache.Set(cacheKey, serializer.Serialize(CreateQuestionnaireDocument(questionnaireId, "cached")));

            QuestionnaireDocument? loaded = null;

            await RunThroughActionFilter(dbContext, HttpMethods.Get, throwInHandler: false,
                handlerBody: () => loaded = storage.GetById(questionnaireId.FormatGuid()));

            Assert.That(loaded?.Title, Is.EqualTo("cached"));
        }

        [Test]
        public async Task when_write_request_uses_transaction_filter_tracked_questionnaire_reads_are_reloaded()
        {
            var dbContext = ServiceLocator.GetInstance<DesignerDbContext>();
            var questionnaireId = Guid.NewGuid();
            SeedQuestionnaireDocument(dbContext, questionnaireId, "stale");

            _ = dbContext.QuestionnaireDocuments.Find(questionnaireId.FormatGuid());
            SeedQuestionnaireDocument(dbContext, questionnaireId, "fresh");

            var serializer = new EntitySerializer<QuestionnaireDocument>();
            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var evictionTokens = new KeyValueCacheEvictionTokens();
            var invalidation = new TransactionalMemoryCacheInvalidation(memoryCache, evictionTokens);
            var storage = new DesignerKeyValueStorage<QuestionnaireDocument>(dbContext, memoryCache, serializer, invalidation, evictionTokens);

            QuestionnaireDocument? loaded = null;

            await RunThroughActionFilter(dbContext, HttpMethods.Post, throwInHandler: false,
                handlerBody: () => loaded = storage.GetById(questionnaireId.FormatGuid()));

            Assert.That(loaded?.Title, Is.EqualTo("fresh"));
        }

        private static void StageAndFlush(DesignerDbContext dbContext, Guid id)
        {
            dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(id, "test"));
            dbContext.SaveChanges();
        }

        private static void SeedQuestionnaireDocument(DesignerDbContext dbContext, Guid questionnaireId, string title)
        {
            using var fresh = new DesignerDbContext(new DbContextOptionsBuilder<DesignerDbContext>()
                .UseNpgsql(dbContext.Database.GetConnectionString())
                .Options);

            var serializer = new EntitySerializer<QuestionnaireDocument>();
            var id = questionnaireId.FormatGuid();
            var stored = fresh.QuestionnaireDocuments.Find(id);
            if (stored == null)
            {
                fresh.QuestionnaireDocuments.Add(new StoredQuestionnaireDocument
                {
                    Id = id,
                    Value = serializer.Serialize(CreateQuestionnaireDocument(questionnaireId, title))
                });
            }
            else
            {
                stored.Value = serializer.Serialize(CreateQuestionnaireDocument(questionnaireId, title));
            }

            fresh.SaveChanges();
        }

        private static QuestionnaireDocument CreateQuestionnaireDocument(Guid questionnaireId, string title)
            => new QuestionnaireDocument
            {
                PublicKey = questionnaireId,
                Title = title,
                HideIfDisabled = true
            };

        private static bool RowExists(DesignerDbContext filterContext, Guid id)
        {
            using var fresh = new DesignerDbContext(new DbContextOptionsBuilder<DesignerDbContext>()
                .UseNpgsql(filterContext.Database.GetConnectionString())
                .Options);

            var key = id.FormatGuid();
            return fresh.Questionnaires.AsNoTracking().Any(x => x.QuestionnaireId == key);
        }

        private static async Task RunThroughActionFilter(DesignerDbContext dbContext, string method, bool throwInHandler, Action handlerBody, ITransactionalMemoryCacheInvalidation? invalidation = null)
        {
            var services = new ServiceCollection()
                .AddSingleton(dbContext)
                .AddSingleton<ITransactionalMemoryCacheInvalidation>(invalidation ?? new NoopCacheInvalidation())
                .AddSingleton<ITransactionRollbackState, TransactionRollbackState>()
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext { RequestServices = services };
            httpContext.Request.Method = method;

            var filters = new List<IFilterMetadata>();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
            var executing = new ActionExecutingContext(actionContext, filters, new Dictionary<string, object?>(), controller: new object());

            ActionExecutionDelegate next = () =>
            {
                handlerBody();
                if (throwInHandler)
                    throw new InvalidOperationException("handler failure");

                return Task.FromResult(new ActionExecutedContext(actionContext, filters, controller: new object()) { Result = new OkResult() });
            };

            await new TransactionFilter().OnActionExecutionAsync(executing, next);
        }

        private sealed class AssertTransactionSettledInvalidation : ITransactionalMemoryCacheInvalidation
        {
            private readonly DesignerDbContext dbContext;
            private readonly Func<bool> rowExists;

            public AssertTransactionSettledInvalidation(DesignerDbContext dbContext, Func<bool> rowExists)
            {
                this.dbContext = dbContext;
                this.rowExists = rowExists;
            }

            public bool FlushCalled { get; private set; }

            public void Enqueue(string cacheKey) { }

            public void Flush()
            {
                FlushCalled = true;
                Assert.That(this.dbContext.Database.CurrentTransaction, Is.Null);
                Assert.That(this.rowExists(), Is.False);
            }
        }

        private sealed class NoopCacheInvalidation : ITransactionalMemoryCacheInvalidation
        {
            public void Enqueue(string cacheKey) { }
            public void Flush() { }
        }
    }
}
