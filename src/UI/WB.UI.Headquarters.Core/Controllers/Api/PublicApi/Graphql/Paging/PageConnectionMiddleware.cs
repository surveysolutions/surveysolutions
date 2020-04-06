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

            PageDetails pageDetails = new PageDetails
            {
                Skip = context.Variables.GetVariable<int>("skip"),
                Take = context.Variables.GetVariable<int>("take"),
            };

            if (context.Result is IQueryable<TClrType> source)
            {
                context.Result = await new PagedConnectionResolver<TClrType, TSchemaType>(source, pageDetails)
                    .ResolveAsync(context.RequestAborted)
                    .ConfigureAwait(false);
            }
        }
    }
}
