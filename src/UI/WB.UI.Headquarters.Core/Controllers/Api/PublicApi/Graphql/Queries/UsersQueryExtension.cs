#nullable enable
using HotChocolate.Types;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Queries
{
    public class UsersQueryExtension : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("HeadquartersQuery");
            
            descriptor.Field<UsersResolver>(x => x.GetViewer(default!))
                .Authorize()
                .Type<UserType>()
                .Name("viewer");
        }
    }
}
