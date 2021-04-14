using HotChocolate.Data.Filters;
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class RolesInputFilterType : FilterInputType
    {
        protected override void Configure(IFilterInputTypeDescriptor descriptor)
        {
            descriptor.Name("RoleFilterInput");
            descriptor.Operation(DefaultFilterOperations.Equals).Type<EnumType<UserRoles>>();
        }
    }
}
