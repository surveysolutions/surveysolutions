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

            descriptor.Field(x => x.Workspaces).Type<NonNullType<ListType<NonNullType<StringType>>>>()
                .Resolver(ctx => ctx.Parent<HqUser>().Workspaces.Select(w => w.Workspace.Name));
        }

        protected static void ConfigureUserFields(IObjectTypeDescriptor<HqUser> descriptor)
        {
            descriptor.Field(x => x.Id).Type<NonNullType<IdType>>();
            descriptor.Field("role").Type<NonNullType<EnumType<UserRoles>>>()
                .Resolver(ctx => ctx.Parent<HqUser>().Role);
            descriptor.Field(x => x.UserName).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.FullName).Type<StringType>();
            descriptor.Field(x => x.Email).Type<StringType>();
            descriptor.Field(x => x.PhoneNumber).Type<StringType>();
            descriptor.Field(x => x.CreationDate).Type<DateType>();
            descriptor.Field("isLocked").Type<BooleanType>()
                .Resolver(r => r.Parent<HqUser>().IsLockedByHeadquaters || r.Parent<HqUser>().IsLockedBySupervisor);
            descriptor.Field("isArchived").Type<BooleanType>()
                .Resolver(r => r.Parent<HqUser>().IsArchived);
        }
    }
}
