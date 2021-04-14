#nullable enable
using System;
using System.Reflection.Metadata.Ecma335;
using HotChocolate;
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Queries
{
    public class UsersQueryExtension : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("HeadquartersQuery");
            
            descriptor.Field<ViewerResolver>(x => x.GetViewer(default!))
                .Authorize()
                .Type<ViewerType>()
                .Name("viewer");

            descriptor.Field<UsersResolver>(x => x.GetUsers(default, default))
                .Authorize()
                .Name("users")
                .Type<ListType<UserType>>()
                .UseSimplePaging<UserType, HqUser>()
                .UseSorting<UsersSortInputType>()
                .UseFiltering<UsersFilterInputType>();

            descriptor.Field<UserResolver>(x => x.GetUser(default, default))
                .Authorize()
                .Name("user")
                .Description("Gets detailed information about single user within workspace")
                .Type<WorkspaceUserType>()
                .HasWorkspace()
                .Argument("id", a => a.Type<NonNullType<IdType>>());
        }
    }

    public class UserResolver
    {
        public HqUser GetUser(Guid id,
            [Service] IUnitOfWork unitOfWork)
        {
            unitOfWork.DiscardChanges();
            return unitOfWork.Session.Get<HqUser>(id);
        }
    }
}
