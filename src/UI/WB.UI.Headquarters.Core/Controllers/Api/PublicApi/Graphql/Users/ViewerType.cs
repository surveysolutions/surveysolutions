using System.Linq;
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class ViewerType : ObjectType<UserDto>
    {
        protected override void Configure(IObjectTypeDescriptor<UserDto> descriptor)
        {
            descriptor.Name("Viewer");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x => x.Id).Type<NonNullType<IdType>>();
            descriptor.Field("role").Type<NonNullType<EnumType<UserRoles>>>()
                .Resolver(ctx => ctx.Parent<HqUser>().Roles.First().Id.ToUserRole());
            descriptor.Field(x => x.UserName).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.Workspaces).Type<NonNullType<ListType<NonNullType<StringType>>>>();
        }
    }
}
