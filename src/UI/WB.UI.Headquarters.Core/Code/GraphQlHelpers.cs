using HotChocolate.Resolvers;

namespace WB.UI.Headquarters.Code
{
    public static class GraphQlHelpers
    {
        public static string GetWorkspace(this IResolverContext ctx)
        {
            var workspaceArgument = ctx.Argument<string>("workspace");
            
            if (workspaceArgument == null)
            {
                ctx.Variables.TryGetVariable("workspace", out workspaceArgument);
            }

            return workspaceArgument;
        }
    }
}
