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
    // an unhandled exception is treated as successful and committed; only safe (read-only) methods and
    // unhandled exceptions trigger rollback. It also starts after authentication, authorization, and model
    // binding.
    public class TransactionFilter : IAsyncActionFilter, IAsyncPageFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (SkipTransaction(context))
            {
                await next();
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<DesignerDbContext>();
            await ExecuteInTransactionAsync(context.HttpContext, dbContext, async () =>
            {
                var executedContext = await next();
                return executedContext.Exception == null;
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
            await ExecuteInTransactionAsync(context.HttpContext, dbContext, async () =>
            {
                var executedContext = await next();
                return executedContext.Exception == null;
            });
        }

        private static bool SkipTransaction(FilterContext context)
            => context.Filters.OfType<NoTransactionAttribute>().Any();

        private static bool IsWriteMethod(string method)
            => HttpMethods.IsPost(method)
               || HttpMethods.IsPut(method)
               || HttpMethods.IsPatch(method)
               || HttpMethods.IsDelete(method);

        private static async Task ExecuteInTransactionAsync(HttpContext httpContext, DesignerDbContext dbContext, Func<Task<bool>> action)
        {
            // If a transaction is already open on this context, its opener owns commit/rollback; just run inside it.
            if (dbContext.Database.CurrentTransaction != null)
            {
                await action();
                return;
            }

            var isWrite = IsWriteMethod(httpContext.Request.Method);
            ExceptionDispatchInfo? capturedException = null;

            try
            {
                await ExecuteOwnedTransactionAsync(dbContext, action, isWrite);
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
            catch when (capturedException != null)
            {
                // Keep the original handler/transaction failure if cleanup also faults.
            }

            capturedException?.Throw();
        }

        private static async Task ExecuteOwnedTransactionAsync(DesignerDbContext dbContext, Func<Task<bool>> action, bool isWrite)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(CancellationToken.None);
            try
            {
                var succeeded = await action();
                if (succeeded && isWrite)
                {
                    await dbContext.SaveChangesAsync(CancellationToken.None);
                    await transaction.CommitAsync(CancellationToken.None);
                }
                else
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    // Rollback reverts the database but not the change tracker; detach so downstream reuse of the scoped context is clean.
                    dbContext.ChangeTracker.Clear();
                }
            }
            catch
            {
                dbContext.ChangeTracker.Clear();
                throw;
            }
        }
    }
}
