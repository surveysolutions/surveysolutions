using HotChocolate.Resolvers;
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class Map : ObjectType<MapBrowseItem>
    {
        protected override void Configure(IObjectTypeDescriptor<MapBrowseItem> descriptor)
        {
            descriptor.Name("Map");
            descriptor.Ignore(x => x.FileName);
            descriptor.Field(x => x.Id).Name("fileName").Description("Map file name");
            descriptor.Field(x => x.Size).Description("Size of map in bytes");
            descriptor.Field(x => x.ImportDate).Description("Date when map was imported on HQ");
            descriptor.Field(x => x.Users)
                .Description("List of users assigned to map")
                .Type<ListType<UserMapObjectType>>();
        }
    }
}
