using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

#nullable enable

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map
{
    public class MapReportType: ObjectType<MapReportView>
    {
        protected override void Configure(IObjectTypeDescriptor<MapReportView> descriptor)
        {
            descriptor.Name("MapReport");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x => x.TotalPoint)
                .Type<NonNullType<IntType>>()
                .Description("Total number of points");

            descriptor.Field(x => x.InitialBounds)
                .Type<GeoBoundsObjectType>()
                .Description("Bounds of the area");

            descriptor.Field(x => x.FeatureCollection)
                .Type<FeatureCollectionObjectType>()
                .Description("Collection of features");
        }
    }
}
