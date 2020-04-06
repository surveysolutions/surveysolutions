using HotChocolate.Types;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging
{
    internal static class PagedExtensions
    {
        public static IObjectFieldDescriptor UseSimplePaging<TSchemaType, TClrType>(
            this IObjectFieldDescriptor descriptor)
            where TSchemaType : class, IOutputType where TClrType : class
        {
            return descriptor
                .AddSimplePagingArguments()
                .Type<PagedConnection<TSchemaType>>()
                .Use<PageConnectionMiddleware<TClrType, TSchemaType>>();
        }

        public static IObjectFieldDescriptor AddSimplePagingArguments(
            this IObjectFieldDescriptor descriptor)
        {
            return descriptor
                .Argument("skip", a => a.Type<IntType>())
                .Argument("take", a => a.Type<IntType>());
        }
    }
}
