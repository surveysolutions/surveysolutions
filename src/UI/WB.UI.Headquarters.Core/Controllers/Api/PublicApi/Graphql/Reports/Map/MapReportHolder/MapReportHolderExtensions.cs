#nullable enable
using HotChocolate.Types;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map.MapReportHolder
{
    internal static class MapReportHolderExtensions
    {
        public static IObjectFieldDescriptor WrapIntoHolder(this IObjectFieldDescriptor descriptor)
        {
            return descriptor
                .Type<MapReportHolder>()
                .Use<MapReportHolderMiddleware<GpsAnswerQuery>>();
        }
    }
}
