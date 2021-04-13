#nullable enable
using System.Reflection.Metadata.Ecma335;
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.User;
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
                .UseSorting<UsersSortInputType>()
                .UseFiltering<UsersFilterInputType>()
                .UseSimplePaging<UserType, HqUser>();
        }
    }
}
