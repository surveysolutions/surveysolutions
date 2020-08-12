using HotChocolate.Types.Sorting;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class MapsSortInputType: SortInputType<MapBrowseItem>
    {
        protected override void Configure(ISortInputTypeDescriptor<MapBrowseItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("MapsSort");
            descriptor.Sortable(x => x.Id).Name("fileName");
            descriptor.Sortable(x => x.ImportDate);
            descriptor.Sortable(x => x.Size);
        }
    }
}
