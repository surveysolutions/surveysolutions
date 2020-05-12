using HotChocolate.Types;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class HeadquartersMutations : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field<MapsResolver>(t => t.DeleteMap(default))
                .Type<Map>()
                .Argument("id", a => a.Description("Map file name").Type<NonNullType<StringType>>());
            
            descriptor.Field<MapsResolver>(t => t.DeleteUserFromMap(default, default))
                .Type<Map>()
                .Argument("id", a => a.Description("Map file name").Type<NonNullType<StringType>>())
                .Argument("userName", a => a.Type<NonNullType<StringType>>());
            
            descriptor.Field<MapsResolver>(t => t.AddUserToMap(default, default))
                .Type<Map>()
                .Argument("id", a => a.Description("Map file name").Type<NonNullType<StringType>>())
                .Argument("userName", a => a.Type<NonNullType<StringType>>());
        }
    }
}
