#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.GenericSubdomains.Portable;
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

        private static void StageAndFlush(DesignerDbContext dbContext, Guid id)
        {
            dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(id, "test"));
            dbContext.SaveChanges();
        }

        private static bool RowExists(DesignerDbContext filterContext, Guid id)
        {
            using var fresh = new DesignerDbContext(new DbContextOptionsBuilder<DesignerDbContext>()
                .UseNpgsql(filterContext.Database.GetConnectionString())
                .Options);

            var key = id.FormatGuid();
            return fresh.Questionnaires.AsNoTracking().Any(x => x.QuestionnaireId == key);
        }

        private static async Task RunThroughActionFilter(DesignerDbContext dbContext, string method, bool throwInHandler, Action handlerBody)
        {
            var services = new ServiceCollection()
                .AddSingleton(dbContext)
                .AddSingleton<ITransactionalMemoryCacheInvalidation>(new NoopCacheInvalidation())
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

        private sealed class NoopCacheInvalidation : ITransactionalMemoryCacheInvalidation
        {
            public void Enqueue(string cacheKey) { }
            public void Flush() { }
        }
    }
}
