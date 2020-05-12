using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging
{
    internal sealed class PageConnectionMiddleware<TClrType, TSchemaType> where TSchemaType : class, IType where TClrType : class
    {
        private readonly FieldDelegate next;

        public PageConnectionMiddleware(FieldDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(IMiddlewareContext context)
        {
            await this.next(context).ConfigureAwait(false);

            PageRequestInfo pageRequestInfo = new PageRequestInfo
            {
                Skip = context.Argument<int?>("skip"),
                Take = context.Argument<int?>("take"),
                HasTotalCount = context.FieldSelection.SelectionSet.HasSelectedField("totalCount"),
                HasFilteredCount =  context.FieldSelection.SelectionSet.HasSelectedField("filteredCount"),
            };

            if (context.Result is IQueryable<TClrType> source)
            {
                var fieldQuery = pageRequestInfo.HasTotalCount 
                    ? await context.Field.Resolver.Invoke(context) as IQueryable<TClrType> 
                    : null;
                
                context.Result = await new PagedConnectionResolver<TClrType, TSchemaType>(fieldQuery, source, pageRequestInfo)
                    .ResolveAsync(context.RequestAborted)
                    .ConfigureAwait(false);
            }
        }
    }
}
