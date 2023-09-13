using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
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

            ConfigureUserFields(descriptor);

            descriptor.Field(x => x.Workspaces)
                .Type<NonNullType<ListType<NonNullType<StringType>>>>()
                .Resolve(ctx => ctx.Parent<HqUser>().Workspaces.Select(w => w.Workspace.Name));
        }

        protected static void ConfigureUserFields(IObjectTypeDescriptor<HqUser> descriptor)
        {
            descriptor.Field(x => x.Id).Type<NonNullType<IdType>>();
            descriptor.Field("role").Type<NonNullType<EnumType<UserRoles>>>()
                .Resolve(ctx => ctx.Parent<HqUser>().Role);
            descriptor.Field(x => x.UserName).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.FullName).Type<StringType>();
            descriptor.Field(x => x.Email).Type<StringType>();
            descriptor.Field(x => x.PhoneNumber).Type<StringType>();
            descriptor.Field(x => x.CreationDate).Type<DateType>();
            descriptor.Field(x => x.IsLocked).Type<BooleanType>();
            descriptor.Field(x => x.IsArchived).Type<BooleanType>();
            descriptor.Field("isRelinkAllowed").Type<BooleanType>()
                .Resolve(ctx => ctx.Parent<HqUser>().Profile.AllowRelinkDate.HasValue);
        }
    }
}
