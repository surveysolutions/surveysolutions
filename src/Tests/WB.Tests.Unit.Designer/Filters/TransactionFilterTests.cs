#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.UI.Designer.Filters;
using WB.UI.Shared.Web.Attributes;

namespace WB.Tests.Unit.Designer.Filters;

// Control-flow coverage for TransactionFilter over the InMemory provider. This exercises the public filter
// methods and the commit/rollback DECISION (whether SaveChanges runs), observed through a second context on the
// same store. The InMemory provider does NOT enforce relational transaction rollback, so true atomicity and
// concurrency (partial-write rollback, duplicate sequences) must be covered by PostgreSQL-backed integration
// tests, not here.
[TestFixture]
[TestOf(typeof(TransactionFilter))]
public class TransactionFilterTests
{
    // ---- Public MVC action filter -------------------------------------------------------------------

    [Test]
    public async Task when_write_request_succeeds_it_commits_staged_writes()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, new OkResult(),
            throwInHandler: false, stageItemId: id);

        StoredIds(db).Should().Contain(id);
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task when_safe_request_stages_writes_they_are_rolled_back()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokeActionAsync(db, invalidation.Object, HttpMethods.Get, new OkResult(),
            throwInHandler: false, stageItemId: id);

        StoredIds(db).Should().NotContain(id);
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task when_write_request_returns_error_status_it_still_commits()
    {
        // The filter no longer inspects the HTTP result/status: only an unhandled exception (or a safe method) rolls back.
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, new NotFoundResult(),
            throwInHandler: false, stageItemId: id);

        StoredIds(db).Should().Contain(id);
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task when_handler_throws_it_rolls_back_and_propagates_and_flushes()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        var act = () => InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, result: null,
            throwInHandler: true, stageItemId: id);

        await act.Should().ThrowAsync<InvalidOperationException>();
        StoredIds(db).Should().NotContain(id);
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task when_marked_no_transaction_the_filter_is_bypassed()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, new OkResult(),
            throwInHandler: false, stageItemId: id,
            filters: new List<IFilterMetadata> { new NoTransactionAttribute() });

        // Bypassed: the filter neither owns a transaction nor publishes cache invalidations.
        invalidation.Verify(x => x.Flush(), Times.Never);
        db.Filter.ChangeTracker.Entries<QuestionnaireListViewItem>().Should().ContainSingle();
    }

    // ---- Public Razor page filter -------------------------------------------------------------------

    [Test]
    public async Task when_write_page_handler_succeeds_it_commits_staged_writes()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokePageAsync(db, invalidation.Object, HttpMethods.Post, new PageResult(), stageItemId: id);

        StoredIds(db).Should().Contain(id);
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task when_safe_page_handler_stages_writes_they_are_rolled_back()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokePageAsync(db, invalidation.Object, HttpMethods.Get, new PageResult(), stageItemId: id);

        StoredIds(db).Should().NotContain(id);
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    // ---- Helpers ------------------------------------------------------------------------------------

    private sealed class TestDatabase
    {
        public required DesignerDbContext Filter { get; init; }
        public required Func<DesignerDbContext> Fresh { get; init; }
    }

    private static TestDatabase NewDatabase()
    {
        var name = Guid.NewGuid().ToString("N");

        DesignerDbContext Build() => new(new DbContextOptionsBuilder<DesignerDbContext>()
            .UseInMemoryDatabase(name)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

        return new TestDatabase { Filter = Build(), Fresh = Build };
    }

    private static IReadOnlyList<string> StoredIds(TestDatabase db)
    {
        using var fresh = db.Fresh();
        return fresh.Questionnaires.AsNoTracking().Select(x => x.QuestionnaireId).ToList();
    }

    private static IServiceProvider Services(DesignerDbContext dbContext, ITransactionalMemoryCacheInvalidation invalidation)
        => new ServiceCollection()
            .AddSingleton(dbContext)
            .AddSingleton(invalidation)
            .BuildServiceProvider();

    private static DefaultHttpContext HttpContextFor(DesignerDbContext dbContext, ITransactionalMemoryCacheInvalidation invalidation, string method)
    {
        var httpContext = new DefaultHttpContext { RequestServices = Services(dbContext, invalidation) };
        httpContext.Request.Method = method;
        return httpContext;
    }

    private static void Stage(DesignerDbContext dbContext, string? stageItemId)
    {
        if (stageItemId != null)
            dbContext.Questionnaires.Add(new QuestionnaireListViewItem { QuestionnaireId = stageItemId, Title = "test" });
    }

    private static async Task InvokeActionAsync(
        TestDatabase db,
        ITransactionalMemoryCacheInvalidation invalidation,
        string method,
        IActionResult? result,
        bool throwInHandler,
        string? stageItemId,
        IList<IFilterMetadata>? filters = null)
    {
        var httpContext = HttpContextFor(db.Filter, invalidation, method);
        var filterList = filters ?? new List<IFilterMetadata>();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var executing = new ActionExecutingContext(actionContext, filterList, new Dictionary<string, object?>(), controller: new object());

        ActionExecutionDelegate next = async () =>
        {
            Stage(db.Filter, stageItemId);
            await Task.Yield();
            if (throwInHandler)
                throw new InvalidOperationException("handler failure");

            return new ActionExecutedContext(actionContext, filterList, controller: new object()) { Result = result };
        };

        await new TransactionFilter().OnActionExecutionAsync(executing, next);
    }

    private static async Task InvokePageAsync(
        TestDatabase db,
        ITransactionalMemoryCacheInvalidation invalidation,
        string method,
        IActionResult? result,
        string? stageItemId,
        IList<IFilterMetadata>? filters = null)
    {
        var httpContext = HttpContextFor(db.Filter, invalidation, method);
        var filterList = filters ?? new List<IFilterMetadata>();
        var actionContext = new ActionContext(httpContext, new RouteData(), new CompiledPageActionDescriptor(), new ModelStateDictionary());
        var pageContext = new PageContext(actionContext);
        var handlerMethod = new HandlerMethodDescriptor();
        var executing = new PageHandlerExecutingContext(pageContext, filterList, handlerMethod, new Dictionary<string, object?>(), handlerInstance: new object());

        PageHandlerExecutionDelegate next = async () =>
        {
            Stage(db.Filter, stageItemId);
            await Task.Yield();
            return new PageHandlerExecutedContext(pageContext, filterList, handlerMethod, handlerInstance: new object()) { Result = result };
        };

        await new TransactionFilter().OnPageHandlerExecutionAsync(executing, next);
    }
}
