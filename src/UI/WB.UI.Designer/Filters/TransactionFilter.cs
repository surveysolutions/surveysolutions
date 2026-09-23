using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Designer.Filters
{
    // Wraps each web request in a single DesignerDbContext transaction so that all writes
    // (change history, questionnaire list, state tracker snapshot) commit or roll back atomically.
    // The transaction outcome follows the action outcome, not the HTTP verb: it commits only when
    // the action completes without an exception and does not return an error (>= 400) result, so a
    // failed command rolls back while a write performed from a GET handler still commits.
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
            await ExecuteInTransactionAsync(dbContext, async () =>
            {
                var executed = await next();
                return IsSuccessfulOutcome(executed.Exception, executed.Result);
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
            await ExecuteInTransactionAsync(dbContext, async () =>
            {
                var executed = await next();
                return IsSuccessfulOutcome(executed.Exception, executed.Result);
            });
        }

        private static bool SkipTransaction(FilterContext context)
            => context.Filters.OfType<NoTransactionAttribute>().Any();

        private static bool IsSuccessfulOutcome(Exception? exception, IActionResult? result)
            => exception == null
               && !(result is IStatusCodeActionResult { StatusCode: >= 400 });

        private static async Task ExecuteInTransactionAsync(DesignerDbContext dbContext, Func<Task<bool>> action)
        {
            // An endpoint may already own a transaction (e.g. the command API); don't nest.
            if (dbContext.Database.CurrentTransaction != null)
            {
                await action();
                return;
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            var succeeded = await action();
            if (succeeded)
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
