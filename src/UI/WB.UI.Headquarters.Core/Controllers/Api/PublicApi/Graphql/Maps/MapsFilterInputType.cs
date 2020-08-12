using HotChocolate.Types.Filters;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class MapsFilterInputType: FilterInputType<MapBrowseItem>
    {
        protected override void Configure(IFilterInputTypeDescriptor<MapBrowseItem> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("MapsFilter");
            
            descriptor.Filter(x => x.FileName)
                .BindFiltersExplicitly()
                .AllowEquals().And().AllowStartsWith().And().AllowIn().And().AllowNotIn();
            
            descriptor.Filter(x => x.ImportDate)
                .BindFiltersExplicitly()
                .AllowGreaterThan()
                .And().AllowGreaterThanOrEquals()
                .And().AllowLowerThan()
                .And().AllowLowerThanOrEquals()
                .And().AllowNotGreaterThan()
                .And().AllowNotGreaterThanOrEquals()
                .And().AllowNotLowerThan()
                .And().AllowNotLowerThanOrEquals();
            
            descriptor.Filter(x => x.Size)
                .BindFiltersExplicitly()
                .AllowGreaterThan()
                .And().AllowGreaterThanOrEquals()
                .And().AllowLowerThan()
                .And().AllowLowerThanOrEquals()
                .And().AllowNotGreaterThan()
                .And().AllowNotGreaterThanOrEquals()
                .And().AllowNotLowerThan()
                .And().AllowNotLowerThanOrEquals();
            
            descriptor.List(x => x.Users)
                .BindExplicitly()
                .AllowSome(y =>
                {
                    y.BindFieldsExplicitly();
                    
                    y.Filter(f => f.UserName)
                        .BindFiltersExplicitly()
                        .AllowEquals().Description("Allows equals comparison of user")
                        .And().AllowNotEquals().Description("Allows not equals comparison of username")
                        .And().AllowStartsWith().Description("Allows starts with comparison of username")
                        .And().AllowNotStartsWith().Description("Allows not starts with comparison of username");
                }).Name("users_some");
        }
    }
}
