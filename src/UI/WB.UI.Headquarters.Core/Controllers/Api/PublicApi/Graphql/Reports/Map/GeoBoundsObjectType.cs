using HotChocolate.Types;
using Supercluster;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map
{
    public class GeoBoundsObjectType : ObjectType<GeoBounds>
    {
        protected override void Configure(IObjectTypeDescriptor<GeoBounds> descriptor)
        {
            descriptor.Name("GeoBounds");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x => x.North)
                .Type<NonNullType<FloatType>>()
                .Description("North bound");
            descriptor.Field(x => x.South)
                .Type<NonNullType<FloatType>>()
                .Description("South bound");
            descriptor.Field(x => x.East)
                .Type<NonNullType<FloatType>>()
                .Description("East bound");
            descriptor.Field(x => x.West)
                .Type<NonNullType<FloatType>>()
                .Description("West bound");
        }
    }
}
