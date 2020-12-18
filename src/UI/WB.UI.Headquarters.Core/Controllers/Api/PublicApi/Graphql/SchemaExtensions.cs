using HotChocolate.Types;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public static class SchemaExtensions
    {
        public static IObjectFieldDescriptor HasWorkspace(this IObjectFieldDescriptor descriptor)
        {
            return descriptor.Argument("workspace",
                a => a.Description("Workspace name").Type<NonNullType<StringType>>()
                    .DefaultValue(WorkspaceConstants.DefaultWorkspaceName));
        }
    }
}
