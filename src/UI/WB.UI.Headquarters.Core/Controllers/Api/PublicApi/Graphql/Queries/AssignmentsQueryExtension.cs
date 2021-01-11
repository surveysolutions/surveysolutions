#nullable enable
using HotChocolate.Types;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Queries
{
    public class AssignmentsQueryExtension : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("HeadquartersQuery");
            
            descriptor.Field<AssignmentsResolver>(x => x.Assignments(default!, default!))
                .Authorize()
                .HasWorkspace()
                .UseSimplePaging<Assignment, Core.BoundedContexts.Headquarters.Assignments.Assignment>()
                .UseFiltering<AssignmentsFilter>();
        }
    }
}
