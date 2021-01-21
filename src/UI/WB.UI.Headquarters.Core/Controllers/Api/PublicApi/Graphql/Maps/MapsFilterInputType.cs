using System;
using HotChocolate.Data.Filters;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class MapsFilterInputType: FilterInputType<MapBrowseItem>
    {
        protected override void Configure(IFilterInputTypeDescriptor<MapBrowseItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("MapsFilter");

            descriptor.Field(x => x.FileName);
            descriptor.Field(x => x.ImportDate).Name("importDateUtc");
            descriptor.Field(x => x.Size);
            descriptor.Field(x => x.Users);
        }
    }
}
