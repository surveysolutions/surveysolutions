using HotChocolate.Data.Filters;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Filters;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Filters
{ 
    public class CustomConventionExtension : FilterConventionExtension
    {
        protected override void Configure(IFilterConventionDescriptor descriptor)
        {
            descriptor.BindRuntimeType<string, CustomStringOperationFilterInput>();
            descriptor.Configure<StringOperationFilterInputType>(
                x => x.Operation(DefaultFilterOperations.Equals).Description("Equals"));
            
            descriptor.Configure<StringOperationFilterInputType>(
                x => x.Operation(DefaultFilterOperations.NotEquals).Description("Not equals"));
            
            descriptor.Configure<StringOperationFilterInputType>(
                x => x.Operation(DefaultFilterOperations.EndsWith).Ignore()); //doesn't work
            
            descriptor.Configure<StringOperationFilterInputType>(
                x => x.Operation(DefaultFilterOperations.EndsWith).Ignore());
        }
    }
}


