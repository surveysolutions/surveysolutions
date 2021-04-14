using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Conventions;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    [PagedTypeName("Users")]
    public class UserType : ObjectType<HqUser>
    {
        protected override void Configure(IObjectTypeDescriptor<HqUser> descriptor)
        {
            descriptor.Name("User");
            descriptor.BindFieldsExplicitly();

            descriptor.Field(x => x.Id).Type<NonNullType<IdType>>();
            descriptor.Field("role").Type<NonNullType<EnumType<UserRoles>>>()
                .Resolver(ctx => ctx.Parent<HqUser>().Role);
            descriptor.Field(x => x.UserName).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.Workspaces).Type<NonNullType<ListType<NonNullType<StringType>>>>();
        }
    }
}
