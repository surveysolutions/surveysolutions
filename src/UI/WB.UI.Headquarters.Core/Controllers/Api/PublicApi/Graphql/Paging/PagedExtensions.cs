#nullable enable
using System.Linq;
using HotChocolate.Language;
using HotChocolate.Types;


namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging
{
    internal static class PagedExtensions
    {
        public static IObjectFieldDescriptor UseSimplePaging<TSchemaType, TClrType>(
            this IObjectFieldDescriptor descriptor)
            where TSchemaType : class, IOutputType where TClrType : class
        {
            var objectFieldDescriptor = descriptor
                .AddSimplePagingArguments()
                .Type<PagedConnection<TSchemaType>>()
                .Use<PageConnectionMiddleware<TClrType, TSchemaType>>();

            return objectFieldDescriptor;
        }

        public static IObjectFieldDescriptor AddSimplePagingArguments(
            this IObjectFieldDescriptor descriptor)
        {
            return descriptor
                .Argument("skip", a => a.Type<IntType>())
                .Argument("take", a => a.Type<IntType>());
        }

        public static bool HasSelectedField(this SelectionSetNode? selection, string fieldName)
        {
            return selection?.Selections.Any(a => a is FieldNode field && field.Name.Value == fieldName) ?? false;
        }
    }
}
