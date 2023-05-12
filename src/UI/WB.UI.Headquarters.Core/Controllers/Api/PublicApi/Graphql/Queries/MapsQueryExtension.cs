#nullable enable
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Queries
{
    public class MapsQueryExtension : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("HeadquartersQuery");
            
            descriptor.Field<MapsResolver>(x => x.GetMaps(default!, default!))
                .Authorize()
                .HasWorkspace()
                .UseSimplePaging<Map, MapBrowseItem>()
                .UseFiltering<MapsFilterInputType>()
                .UseSorting<MapsSortInputType>();
        }
    }
}
