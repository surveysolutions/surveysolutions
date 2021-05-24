using HotChocolate;
using HotChocolate.Resolvers;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code
{
    public static class GraphQlHelpers
    {
        public static bool HasWorkspaceArgument(this IMiddlewareContext ctx, out string workspace)
        {
            ctx.Variables.TryGetVariable("workspace", out string workspaceValue);

            if (workspaceValue == null)
            {
                try
                {
                    if (ctx.Field.Arguments.ContainsField("workspace"))
                        workspaceValue = ctx.ArgumentValue<string>("workspace");
                    else
                    {
                        workspace = null;
                        return false;
                    }
                }
                catch (GraphQLException)
                {
                    workspace = null;
                    return false;
                }
            }

            workspace = workspaceValue;
            return true;
        }
    }
}
