using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class UserType : ObjectType<UserDto>
    {
        protected override void Configure(IObjectTypeDescriptor<UserDto> descriptor)
        {
            descriptor.Name("User");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x => x.Id).Type<IdType>();
            descriptor.Field(x => x.Roles).Type<NonNullType<ListType<EnumType<UserRoles>>>>();
            descriptor.Field(x => x.UserName).Type<NonNullType<StringType>>();
        }
    }
}
