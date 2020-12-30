using HotChocolate;
using HotChocolate.Resolvers;

namespace WB.UI.Headquarters.Code
{
    public static class GraphQlHelpers
    {
        public static string GetWorkspace(this IResolverContext ctx)
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
                    workspaceArgument = null;
                }
            }
            
            return workspaceArgument;
        }
    }
}
