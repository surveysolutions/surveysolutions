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
    // rollback-only; safe (read-only) methods, short-circuited handlers, and unhandled exceptions trigger rollback.
    // It also starts after
    // authentication, authorization, and model binding.
    public class TransactionFilter : IAsyncActionFilter, IAsyncPageFilter
    {
        // MVC usually reports a handler failure through the executed context instead of throwing from next(),
        // so the exception is carried alongside the commit decision: cleanup faults must never replace it.
        private readonly record struct HandlerOutcome(bool ShouldCommit, Exception? HandlerException);

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
                // Canceled means an inner filter short-circuited: the handler never ran, so nothing may commit.
                var shouldCommit = executedContext.Exception == null && !executedContext.Canceled
                    && !rollbackState.IsRollbackOnly;
                return new HandlerOutcome(shouldCommit, executedContext.Exception);
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
                var shouldCommit = executedContext.Exception == null && !executedContext.Canceled
                    && !rollbackState.IsRollbackOnly;
                return new HandlerOutcome(shouldCommit, executedContext.Exception);
            });
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
            Exception? handlerException = null;
            var committed = false;

            try
            {
                (handlerException, committed) = await ExecuteOwnedTransactionAsync(dbContext, action, isWrite);
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
            catch when (capturedException != null || handlerException != null)
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

        // Returns the handler exception MVC delivered without throwing, so the caller can tell a primary failure
        // from a cleanup failure. Rethrows only failures that MVC is not already about to surface itself.
        private static async Task<(Exception? HandlerException, bool Committed)> ExecuteOwnedTransactionAsync(DesignerDbContext dbContext, Func<Task<HandlerOutcome>> action, bool isWrite)
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
            catch when (transactionFailure != null || outcome.HandlerException != null)
            {
                // Disposal must not replace a failure that is already on its way to the caller.
            }

            if (outcome.HandlerException == null)
                transactionFailure?.Throw();

            return (outcome.HandlerException, committed);
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
