using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class WorkspaceUserType : UserType
    {
        protected override void Configure(IObjectTypeDescriptor<HqUser> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Name("WorkspaceUser");
            descriptor.Field("supervisorId").Type<UuidType>()
                .Resolver(r => r.Parent<HqUser>().WorkspaceProfile?.SupervisorId);
        }
    }
}
