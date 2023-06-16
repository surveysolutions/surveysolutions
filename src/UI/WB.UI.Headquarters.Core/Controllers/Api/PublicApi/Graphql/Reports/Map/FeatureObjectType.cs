using GeoJSON.Net;
using GeoJSON.Net.Feature;
using HotChocolate.Types;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map
{
    public class FeatureObjectType : ObjectType<Feature>
    {
        protected override void Configure(IObjectTypeDescriptor<Feature> descriptor)
        {
            descriptor.Name("Feature");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x => x.Type)
                .Type<NonNullType<EnumType<GeoJSONObjectType>>>()
                .Description("Type of the feature");
            descriptor.Field(x => x.Id)
                .Type<StringType>()
                .Description("Id of the feature");
            descriptor.Field(x => x.Geometry)
                .Type<AnyType>()
                .Description("Geometry of the feature");
            descriptor.Field(x => x.Properties)
                .Type<AnyType>()
                .Description("Properties of the feature");
        }
    }
}
