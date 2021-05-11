using HotChocolate.Data.Filters;
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class UsersFilterInputType : FilterInputType<HqUser>
    {
        protected override void Configure(IFilterInputTypeDescriptor<HqUser> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("UsersFilterInput");
            descriptor.Field(x => x.UserName);
            descriptor.Field(x => x.FullName);
            descriptor.Field(x => x.IsArchived);
            descriptor.Field(x => x.IsLocked);
            descriptor.Field(x => x.CreationDate);
            descriptor.Field(x => x.Email);
            descriptor.Field(x => x.PhoneNumber);
            descriptor.Field(x => x.Id);
            descriptor.Field(x => x.Role).Type<RolesInputFilterType>();
            descriptor.Field(x => x.Workspaces);
        }
    }
}
