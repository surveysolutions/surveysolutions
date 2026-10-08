using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Designer.Filters
{
    // Wraps the MVC action / Razor page handler in a single DesignerDbContext transaction so that the
    // writes of one business operation (change history, questionnaire list, state tracker snapshot) commit
    // or roll back atomically. The boundary is handler completion, NOT the whole request: commit happens
    // after the handler returns but BEFORE result execution, so view rendering, JSON serialization, result
    // filters, and any writes/errors produced while the result executes are outside this transaction.
    // Because of that timing, the filter does NOT inspect IActionResult types, result status codes, or
    // HttpResponse.StatusCode when deciding to commit. For write requests, any handler that returns without
    // an unhandled exception is treated as successful and committed unless the handler marks the request
    // rollback-only; safe (read-only) methods, short-circuited handlers, and handler exceptions (handled or not) trigger rollback.
    // It also starts after
    // authentication, authorization, and model binding.
    public class TransactionFilter : IAsyncActionFilter, IAsyncPageFilter
    {
        // MVC usually reports a handler failure through the executed context instead of throwing from next(). Only an
        // unhandled one is rethrown by MVC, so only that one is carried as pending: cleanup faults must never replace it.
        private readonly record struct HandlerOutcome(bool ShouldCommit, Exception? PendingException);

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (SkipTransaction(context))
            {
                await next();
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<DesignerDbContext>();
            var rollbackState = context.HttpContext.RequestServices.GetRequiredService<ITransactionRollbackState>();
            await ExecuteInTransactionAsync(context.HttpContext, dbContext, async () =>
            {
                var executedContext = await next();
                return Outcome(executedContext.Exception, executedContext.ExceptionHandled, executedContext.Canceled, rollbackState);
            });
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            if (SkipTransaction(context))
            {
                await next();
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<DesignerDbContext>();
            var rollbackState = context.HttpContext.RequestServices.GetRequiredService<ITransactionRollbackState>();
            await ExecuteInTransactionAsync(context.HttpContext, dbContext, async () =>
            {
                var executedContext = await next();
                return Outcome(executedContext.Exception, executedContext.ExceptionHandled, executedContext.Canceled, rollbackState);
            });
        }

        // Canceled means an inner filter short-circuited: the handler never ran, so nothing may commit.
        // A handled exception (kept or cleared) still means the handler failed, so it rolls back but is not pending.
        private static HandlerOutcome Outcome(Exception? exception, bool exceptionHandled, bool canceled, ITransactionRollbackState rollbackState)
        {
            var shouldCommit = exception == null && !exceptionHandled && !canceled && !rollbackState.IsRollbackOnly;
            return new HandlerOutcome(shouldCommit, exceptionHandled ? null : exception);
        }

        private static bool SkipTransaction(FilterContext context)
            => context.Filters.OfType<NoTransactionAttribute>().Any();

        private static bool IsWriteMethod(string method)
            => HttpMethods.IsPost(method)
               || HttpMethods.IsPut(method)
               || HttpMethods.IsPatch(method)
               || HttpMethods.IsDelete(method);

        private static async Task ExecuteInTransactionAsync(HttpContext httpContext, DesignerDbContext dbContext, Func<Task<HandlerOutcome>> action)
        {
            var isWrite = IsWriteMethod(httpContext.Request.Method);

            // Running inside a transaction someone else commits would silently drop every guarantee below:
            // rollback-only marks, short-circuits and safe-method rollbacks would all be decided by that owner.
            if (dbContext.Database.CurrentTransaction != null)
                throw new InvalidOperationException(
                    $"{nameof(TransactionFilter)} cannot run inside an externally owned transaction. " +
                    $"Mark the endpoint with [{nameof(NoTransactionAttribute)}] when it manages its own transaction.");

            ExceptionDispatchInfo? capturedException = null;
            Exception? pendingException = null;
            var committed = false;

            try
            {
                (pendingException, committed, capturedException) = await ExecuteOwnedTransactionAsync(dbContext, action, isWrite);
            }
            catch (Exception exception)
            {
                capturedException = ExceptionDispatchInfo.Capture(exception);
            }

            try
            {
                // Publish cache invalidations only once the transaction has fully committed or rolled back.
                httpContext.RequestServices.GetRequiredService<ITransactionalMemoryCacheInvalidation>().Flush();
            }
            catch when (capturedException != null || pendingException != null)
            {
                // Keep the original handler/transaction failure if cleanup also faults.
            }
            finally
            {
                var postCommitActions = httpContext.RequestServices.GetRequiredService<IPostCommitActions>();
                if (committed)
                    await postCommitActions.ExecuteAsync();
                else
                    postCommitActions.Discard();
            }

            capturedException?.Throw();
        }

        // Return the commit outcome alongside failures: a disposal failure must not erase a successful commit
        // and discard its notifications. The caller rethrows failures MVC is not already about to surface.
        private static async Task<(Exception? PendingException, bool Committed, ExceptionDispatchInfo? Failure)> ExecuteOwnedTransactionAsync(DesignerDbContext dbContext, Func<Task<HandlerOutcome>> action, bool isWrite)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(CancellationToken.None);
            HandlerOutcome outcome = default;
            ExceptionDispatchInfo? transactionFailure = null;
            var committed = false;

            try
            {
                outcome = await ExecuteWithSharedCachePolicyAsync(dbContext, isWrite, action);
                if (outcome.ShouldCommit && isWrite)
                {
                    await dbContext.SaveChangesAsync(CancellationToken.None);
                    await transaction.CommitAsync(CancellationToken.None);
                    committed = true;
                }
                else
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    // Rollback reverts the database but not the change tracker; detach so downstream reuse of the scoped context is clean.
                    dbContext.ChangeTracker.Clear();
                }
            }
            catch (Exception exception)
            {
                dbContext.ChangeTracker.Clear();
                transactionFailure = ExceptionDispatchInfo.Capture(exception);
            }

            try
            {
                await transaction.DisposeAsync();
            }
            catch (Exception exception)
            {
                // Disposal must not replace a failure that is already on its way to the caller.
                transactionFailure ??= ExceptionDispatchInfo.Capture(exception);
            }

            return (outcome.PendingException, committed, outcome.PendingException == null ? transactionFailure : null);
        }

        private static async Task<T> ExecuteWithSharedCachePolicyAsync<T>(DesignerDbContext dbContext, bool bypassSharedCache, Func<Task<T>> action)
        {
            var previousValue = dbContext.BypassSharedKeyValueCacheInCurrentTransaction;
            dbContext.BypassSharedKeyValueCacheInCurrentTransaction = previousValue || bypassSharedCache;

            try
            {
                return await action();
            }
            finally
            {
                dbContext.BypassSharedKeyValueCacheInCurrentTransaction = previousValue;
            }
        }
    }
}
