#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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
        // Status-bearing results are only for the caller: the filter decides before result execution and
        // still commits any staged writes unless the handler throws or the method is treated as safe.
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, new NotFoundResult(),
            throwInHandler: false, stageItemId: id);

        StoredIds(db).Should().Contain(id);
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task when_write_request_is_marked_rollback_only_it_rolls_back()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, new BadRequestResult(),
            throwInHandler: false, stageItemId: id, markRollbackOnly: true);

        StoredIds(db).Should().NotContain(id);
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task when_write_request_is_short_circuited_staged_writes_are_rolled_back()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, new ForbidResult(),
            throwInHandler: false, stageItemId: id, canceled: true);

        StoredIds(db).Should().NotContain(id);
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
    public async Task when_handler_and_flush_throw_it_preserves_the_original_exception()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        invalidation.Setup(x => x.Flush()).Throws(new ApplicationException("flush failure"));

        var act = () => InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, result: null,
            throwInHandler: true, stageItemId: Guid.NewGuid().ToString("N"));

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Be("handler failure");
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task when_executed_context_carries_the_handler_exception_it_rolls_back_without_rethrowing()
    {
        // MVC reports a handler failure through ActionExecutedContext.Exception instead of throwing from next();
        // it rethrows once the whole filter chain has run, so this filter must not throw on its own here.
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, result: null,
            throwInHandler: false, stageItemId: id,
            exceptionInExecutedContext: new InvalidOperationException("handler failure"));

        StoredIds(db).Should().NotContain(id);
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    [Test]
    public async Task when_executed_context_carries_the_handler_exception_and_flush_throws_the_cleanup_failure_is_suppressed()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        invalidation.Setup(x => x.Flush()).Throws(new ApplicationException("flush failure"));

        var act = () => InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, result: null,
            throwInHandler: false, stageItemId: Guid.NewGuid().ToString("N"),
            exceptionInExecutedContext: new InvalidOperationException("handler failure"));

        // Throwing the flush failure here would replace the handler exception MVC is about to surface.
        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task when_flush_throws_without_a_pending_failure_it_propagates()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        invalidation.Setup(x => x.Flush()).Throws(new ApplicationException("flush failure"));

        var act = () => InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, new OkResult(),
            throwInHandler: false, stageItemId: Guid.NewGuid().ToString("N"));

        await act.Should().ThrowAsync<ApplicationException>();
    }

    // A handled exception is never rethrown by MVC, so it must roll back without masking cleanup failures.
    [TestCase(true, TestName = "when_inner_filter_handles_exception_and_keeps_it_it_rolls_back_and_surfaces_cleanup_failure")]
    [TestCase(false, TestName = "when_inner_filter_handles_exception_and_clears_it_it_rolls_back_and_surfaces_cleanup_failure")]
    public async Task handled_exception_in_action(bool keepException)
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        invalidation.Setup(x => x.Flush()).Throws(new ApplicationException("flush failure"));
        var id = Guid.NewGuid().ToString("N");

        var act = () => InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, new OkResult(),
            throwInHandler: false, stageItemId: id,
            exceptionInExecutedContext: keepException ? new InvalidOperationException("handled failure") : null,
            exceptionHandled: true);

        await act.Should().ThrowAsync<ApplicationException>();
        StoredIds(db).Should().NotContain(id);
    }

    [TestCase(true, TestName = "when_inner_page_filter_handles_exception_and_keeps_it_it_rolls_back_and_surfaces_cleanup_failure")]
    [TestCase(false, TestName = "when_inner_page_filter_handles_exception_and_clears_it_it_rolls_back_and_surfaces_cleanup_failure")]
    public async Task handled_exception_in_page(bool keepException)
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        invalidation.Setup(x => x.Flush()).Throws(new ApplicationException("flush failure"));
        var id = Guid.NewGuid().ToString("N");

        var act = () => InvokePageAsync(db, invalidation.Object, HttpMethods.Post, new PageResult(), stageItemId: id,
            exceptionInExecutedContext: keepException ? new InvalidOperationException("handled failure") : null,
            exceptionHandled: true);

        await act.Should().ThrowAsync<ApplicationException>();
        StoredIds(db).Should().NotContain(id);
    }

    [Test]
    public async Task when_page_executed_context_carries_the_handler_exception_it_rolls_back_without_rethrowing()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokePageAsync(db, invalidation.Object, HttpMethods.Post, result: null, stageItemId: id,
            exceptionInExecutedContext: new InvalidOperationException("handler failure"));

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

    // ---- Post-commit actions ------------------------------------------------------------------------

    [Test]
    public async Task when_write_request_commits_post_commit_actions_run_after_commit()
    {
        var db = NewDatabase();
        var id = Guid.NewGuid().ToString("N");
        bool? committedWhenActionRan = null;

        await InvokeActionAsync(db, Mock.Of<ITransactionalMemoryCacheInvalidation>(), HttpMethods.Post, new OkResult(),
            throwInHandler: false, stageItemId: id,
            postCommitAction: () =>
            {
                committedWhenActionRan = db.Filter.Database.CurrentTransaction == null && StoredIds(db).Contains(id);
                return Task.CompletedTask;
            });

        committedWhenActionRan.Should().BeTrue();
    }

    [Test]
    public async Task when_write_request_is_marked_rollback_only_post_commit_actions_are_discarded()
    {
        var db = NewDatabase();
        var actionRan = false;

        await InvokeActionAsync(db, Mock.Of<ITransactionalMemoryCacheInvalidation>(), HttpMethods.Post, new BadRequestResult(),
            throwInHandler: false, stageItemId: Guid.NewGuid().ToString("N"), markRollbackOnly: true,
            postCommitAction: () => { actionRan = true; return Task.CompletedTask; });

        actionRan.Should().BeFalse();
    }

    [Test]
    public async Task when_handler_throws_post_commit_actions_are_discarded()
    {
        var db = NewDatabase();
        var actionRan = false;

        var act = () => InvokeActionAsync(db, Mock.Of<ITransactionalMemoryCacheInvalidation>(), HttpMethods.Post, result: null,
            throwInHandler: true, stageItemId: Guid.NewGuid().ToString("N"),
            postCommitAction: () => { actionRan = true; return Task.CompletedTask; });

        await act.Should().ThrowAsync<InvalidOperationException>();
        actionRan.Should().BeFalse();
    }

    [Test]
    public async Task when_post_commit_action_fails_the_committed_request_does_not_fail()
    {
        var db = NewDatabase();
        var id = Guid.NewGuid().ToString("N");

        var act = () => InvokeActionAsync(db, Mock.Of<ITransactionalMemoryCacheInvalidation>(), HttpMethods.Post, new OkResult(),
            throwInHandler: false, stageItemId: id,
            postCommitAction: () => throw new ApplicationException("smtp failure"));

        await act.Should().NotThrowAsync();
        StoredIds(db).Should().Contain(id);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task when_disposal_fails_after_commit_notifications_run_and_disposal_failure_is_preserved(bool flushFails)
    {
        var events = new List<string>();
        var disposalFailure = new ApplicationException("disposal failure");
        var transaction = new Mock<IDbContextTransaction>();
        transaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => events.Add("commit"))
            .Returns(Task.CompletedTask);
        transaction.Setup(x => x.DisposeAsync())
            .Callback(() => events.Add("dispose"))
            .Returns(() => new ValueTask(Task.FromException(disposalFailure)));
        var db = NewDatabase(transaction.Object);
        var id = Guid.NewGuid().ToString("N");
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        invalidation.Setup(x => x.Flush()).Callback(() =>
        {
            events.Add("flush");
            if (flushFails)
                throw new InvalidOperationException("flush failure");
        });

        var act = () => InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, new OkResult(),
            throwInHandler: false, stageItemId: id,
            postCommitAction: () => { events.Add("notification"); return Task.CompletedTask; });

        var exception = await act.Should().ThrowAsync<ApplicationException>();
        exception.Which.Should().BeSameAs(disposalFailure);
        events.Should().Equal("commit", "dispose", "flush", "notification");
        StoredIds(db).Should().Contain(id);
        transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task when_commit_and_disposal_fail_notifications_are_discarded_and_commit_failure_is_preserved()
    {
        var commitFailure = new InvalidOperationException("commit failure");
        var transaction = new Mock<IDbContextTransaction>();
        transaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitFailure);
        transaction.Setup(x => x.DisposeAsync())
            .Returns(() => new ValueTask(Task.FromException(new ApplicationException("disposal failure"))));
        var db = NewDatabase(transaction.Object);
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var actionRan = false;

        var act = () => InvokeActionAsync(db, invalidation.Object, HttpMethods.Post, new OkResult(),
            throwInHandler: false, stageItemId: Guid.NewGuid().ToString("N"),
            postCommitAction: () => { actionRan = true; return Task.CompletedTask; });

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Should().BeSameAs(commitFailure);
        actionRan.Should().BeFalse();
        transaction.Verify(x => x.DisposeAsync(), Times.Once);
        invalidation.Verify(x => x.Flush(), Times.Once);
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

    [Test]
    public async Task when_write_page_handler_is_short_circuited_staged_writes_are_rolled_back()
    {
        var db = NewDatabase();
        var invalidation = new Mock<ITransactionalMemoryCacheInvalidation>();
        var id = Guid.NewGuid().ToString("N");

        await InvokePageAsync(db, invalidation.Object, HttpMethods.Post, new PageResult(), stageItemId: id, canceled: true);

        StoredIds(db).Should().NotContain(id);
        invalidation.Verify(x => x.Flush(), Times.Once);
    }

    // ---- Helpers ------------------------------------------------------------------------------------

    private sealed class TestDatabase
    {
        public required DesignerDbContext Filter { get; init; }
        public required Func<DesignerDbContext> Fresh { get; init; }
    }

    private static TestDatabase NewDatabase(IDbContextTransaction? transaction = null)
    {
        var name = Guid.NewGuid().ToString("N");

        var options = new DbContextOptionsBuilder<DesignerDbContext>()
            .UseInMemoryDatabase(name)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        DesignerDbContext Build() => new(options);

        if (transaction != null)
        {
            // Keep real change tracking/SaveChanges, but inject transaction lifecycle faults independently
            // of the InMemory provider (which does not implement relational transactions).
            var context = new Mock<DesignerDbContext>(options) { CallBase = true };
            var database = new Mock<DatabaseFacade>(context.Object);
            database.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
            context.SetupGet(x => x.Database).Returns(database.Object);
            return new TestDatabase { Filter = context.Object, Fresh = Build };
        }

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
            .AddSingleton<ITransactionRollbackState, TransactionRollbackState>()
            .AddSingleton<IPostCommitActions>(new PostCommitActions(NullLogger<PostCommitActions>.Instance))
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
        IList<IFilterMetadata>? filters = null,
        bool canceled = false,
        bool markRollbackOnly = false,
        Exception? exceptionInExecutedContext = null,
        Func<Task>? postCommitAction = null,
        bool exceptionHandled = false)
    {
        var httpContext = HttpContextFor(db.Filter, invalidation, method);
        var filterList = filters ?? new List<IFilterMetadata>();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var executing = new ActionExecutingContext(actionContext, filterList, new Dictionary<string, object?>(), controller: new object());

        ActionExecutionDelegate next = async () =>
        {
            Stage(db.Filter, stageItemId);
            if (postCommitAction != null)
                httpContext.RequestServices.GetRequiredService<IPostCommitActions>().Enqueue(postCommitAction);
            if (markRollbackOnly)
                httpContext.RequestServices.GetRequiredService<ITransactionRollbackState>().MarkRollbackOnly();
            await Task.Yield();
            if (throwInHandler)
                throw new InvalidOperationException("handler failure");

            return new ActionExecutedContext(actionContext, filterList, controller: new object())
            {
                Result = result,
                Canceled = canceled,
                Exception = exceptionInExecutedContext,
                ExceptionHandled = exceptionHandled
            };
        };

        await new TransactionFilter().OnActionExecutionAsync(executing, next);
    }

    private static async Task InvokePageAsync(
        TestDatabase db,
        ITransactionalMemoryCacheInvalidation invalidation,
        string method,
        IActionResult? result,
        string? stageItemId,
        IList<IFilterMetadata>? filters = null,
        bool canceled = false,
        Exception? exceptionInExecutedContext = null,
        bool exceptionHandled = false)
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
            return new PageHandlerExecutedContext(pageContext, filterList, handlerMethod, handlerInstance: new object())
            {
                Result = result,
                Canceled = canceled,
                Exception = exceptionInExecutedContext,
                ExceptionHandled = exceptionHandled
            };
        };

        await new TransactionFilter().OnPageHandlerExecutionAsync(executing, next);
    }
}
