using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map.MapReportHolder
{
    internal sealed class MapReportHolderMiddleware<TClrType, TSchemaType> where TSchemaType : class, IType where TClrType : class
    {
        private readonly FieldDelegate next;

        public MapReportHolderMiddleware(FieldDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(IMiddlewareContext context)
        {
            await this.next(context).ConfigureAwait(false);

            if (context.Result is IQueryable<TClrType> source)
            {
                context.Result = await new MapReportHolderResolver<TClrType, TSchemaType>(source)
                    .ResolveAsync(context.RequestAborted)
                    .ConfigureAwait(false);
            }
        }
    }
}
