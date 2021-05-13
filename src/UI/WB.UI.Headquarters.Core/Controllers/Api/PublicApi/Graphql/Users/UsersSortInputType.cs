using HotChocolate.Data.Sorting;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class UsersSortInputType: SortInputType<HqUser>
    {
        protected override void Configure(ISortInputTypeDescriptor<HqUser> descriptor)
        {
            descriptor.BindFieldsExplicitly();
            descriptor.Name("UsersSortInput");
            
            descriptor.Field(x => x.UserName);
            descriptor.Field(x => x.CreationDate);
            descriptor.Field(x => x.FullName);
            descriptor.Field(x => x.Role);
            descriptor.Field(x => x.Email);
            descriptor.Field(x => x.PhoneNumber);
            descriptor.Field(x => x.IsLocked);
            descriptor.Field(x => x.IsArchived);
        }
    }
}
