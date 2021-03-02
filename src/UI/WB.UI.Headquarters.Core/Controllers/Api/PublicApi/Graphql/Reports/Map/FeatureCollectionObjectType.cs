using GeoJSON.Net;
using GeoJSON.Net.Feature;
using HotChocolate.Types;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map
{
    public class FeatureCollectionObjectType : ObjectType<FeatureCollection>
    {
        protected override void Configure(IObjectTypeDescriptor<FeatureCollection> descriptor)
        {
            descriptor.Name("FeatureCollection");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x=> x.Features)
                    .Type<ListType<FeatureObjectType>>()
                .Description("List of the features");

            descriptor.Field(x=> x.Type)
                .Type<NonNullType<EnumType<GeoJSONObjectType>>>()
                .Description("List of the features");
        }
    }
}
