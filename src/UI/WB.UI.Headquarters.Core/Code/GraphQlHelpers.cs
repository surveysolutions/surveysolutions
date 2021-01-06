using HotChocolate;
using HotChocolate.Resolvers;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code
{
    public static class GraphQlHelpers
    {
        public static string GetWorkspaceNameOrDefault(this IResolverContext ctx)
        {
            ctx.Variables.TryGetVariable("workspace", out string workspaceArgument);

            if (workspaceArgument == null)
            {
                try
                {
                    workspaceArgument = ctx.ArgumentValue<string>("workspace");
                }
                catch (GraphQLException)
                {
                    // now HotChocolate throws exception instead of null
                    workspaceArgument = WorkspaceConstants.DefaultWorkspaceName;
                }
            }
            
            return workspaceArgument;
        }
    }
}
