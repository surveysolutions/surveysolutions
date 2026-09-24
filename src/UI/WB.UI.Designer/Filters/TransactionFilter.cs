using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    // It also starts after authentication, authorization, and model binding. Safe (read-only) requests are
    // always rolled back so accidental writes never persist.
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
                }
            }
            finally
            {
                // Publish cache invalidations only once the transaction has settled, so no rolled-back state remains cached.
                httpContext.RequestServices.GetRequiredService<ITransactionalMemoryCacheInvalidation>().Flush();
            }
        }
    }
}
