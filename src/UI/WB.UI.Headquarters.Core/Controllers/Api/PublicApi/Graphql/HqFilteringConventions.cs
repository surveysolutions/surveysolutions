using HotChocolate.Data;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    internal class HqFilteringConventions : FilterConvention
    {
        protected override void Configure(IFilterConventionDescriptor descriptor)
        {
            descriptor.AddDefaults();
            descriptor.Provider(
                new QueryableFilterProvider(
                    x => x
                        .AddDefaultFieldHandlers()
                        .AddFieldHandler<RoleEqualsEnumHandler>()
                        // .AddFieldHandler<RolesObjectHandler>()
                        ));
        }
    }
}
