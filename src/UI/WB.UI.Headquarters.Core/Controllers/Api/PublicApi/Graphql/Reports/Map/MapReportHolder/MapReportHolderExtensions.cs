#nullable enable
using HotChocolate.Types;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map.MapReportHolder
{
    internal static class MapReportHolderExtensions
    {
        public static IObjectFieldDescriptor WrapIntoHolder<TSchemaType, TClrType>(
            this IObjectFieldDescriptor descriptor)
            where TSchemaType : class, IOutputType where TClrType : class
        {
            return descriptor
                .Type<MapReportHolder<TSchemaType>>()
                .Use<MapReportHolderMiddleware<TClrType, TSchemaType>>();
        }
    }
}
