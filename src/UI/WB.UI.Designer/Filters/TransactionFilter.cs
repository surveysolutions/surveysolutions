using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Designer.Filters
{
    // Wraps each web request in a single DesignerDbContext transaction so that all writes
    // (change history, questionnaire list, state tracker snapshot) commit or roll back atomically.
    // Safe (read-only) requests are always rolled back so accidental writes never persist.
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
            await ExecuteInTransactionAsync(context.HttpContext, dbContext, async () => (await next()).Exception == null);
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
            await ExecuteInTransactionAsync(context.HttpContext, dbContext, async () => (await next()).Exception == null);
        }

        private static bool SkipTransaction(FilterContext context)
            => context.Filters.OfType<NoTransactionAttribute>().Any();

        private static bool IsReadOnlyMethod(string method)
            => HttpMethods.IsGet(method)
               || HttpMethods.IsHead(method)
               || HttpMethods.IsOptions(method)
               || HttpMethods.IsTrace(method);

        private static async Task ExecuteInTransactionAsync(HttpContext httpContext, DesignerDbContext dbContext, Func<Task<bool>> action)
        {
            // An endpoint may already own a transaction (e.g. the command API); don't nest.
            if (dbContext.Database.CurrentTransaction != null)
            {
                await action();
                return;
            }

            var isReadOnly = IsReadOnlyMethod(httpContext.Request.Method);

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            var succeeded = await action();
            if (succeeded && !isReadOnly)
            {
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync();
            }
        }
    }
}
