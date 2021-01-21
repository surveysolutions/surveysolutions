using HotChocolate.Data.Sorting;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class MapsSortInputType: SortInputType<MapBrowseItem>
    {
        protected override void Configure(ISortInputTypeDescriptor<MapBrowseItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("MapsSort");
            
            descriptor.Field(x => x.Id).Name("fileName");
            descriptor.Field(x => x.ImportDate).Name("importDateUtc");
            descriptor.Field(x => x.Size);
        }
    }
}
