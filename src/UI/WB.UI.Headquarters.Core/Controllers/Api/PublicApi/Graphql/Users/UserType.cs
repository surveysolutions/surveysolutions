using HotChocolate.Types;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class UserType : ObjectType<UserDto>
    {
        protected override void Configure(IObjectTypeDescriptor<UserDto> descriptor)
        {
            descriptor.Name("User");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x => x.Id).Type<NonNullType<IdType>>();
            descriptor.Field(x => x.Roles).Type<NonNullType<ListType<NonNullType<EnumType<UserRoles>>>>>();
            descriptor.Field(x => x.UserName).Type<NonNullType<StringType>>();
            
            descriptor.Field(x => x.Workspaces).Type<NonNullType<ListType<NonNullType<StringType>>>>();
        }
    }
}
