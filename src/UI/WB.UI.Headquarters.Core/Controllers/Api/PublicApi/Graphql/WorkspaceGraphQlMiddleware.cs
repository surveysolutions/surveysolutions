#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class WorkspaceGraphQlMiddleware
    {
        private readonly FieldDelegate next;

        public WorkspaceGraphQlMiddleware(FieldDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(IMiddlewareContext context,
            IWorkspaceContextAccessor workspaceContextAccessor,
            IWorkspaceContextSetter workspaceContextSetter)
        {
            var currentWorkspace = workspaceContextAccessor.CurrentWorkspace();
            var workspace = currentWorkspace?.Name;
            var hasWorkspaceParameter = TryGetWorkspaceName(context, out var workspaceValue);
            
            if (currentWorkspace == null)
            {
                workspace = workspaceValue ?? WorkspaceConstants.DefaultWorkspaceName;

                if (workspace != null)
                {
                    try
                    {
                        workspaceContextSetter.Set(workspace);
                    }
                    catch (MissingWorkspaceException)
                    {
                        context.ReportError(Workspaces.WorkspaceAccessDisabledReason);
                        return;
                    }
                }
            }

            if (hasWorkspaceParameter && workspace != null && !HasUserAccessToWorkspace(context, workspace))
                context.ReportError(Workspaces.WorkspaceAccessDisabledReason);
            
            if (!context.HasErrors)
                await next(context);
        }
        
        public bool TryGetWorkspaceName(IMiddlewareContext ctx, out string? workspace)
        {
            if (ctx.Variables.TryGetVariable("workspace", out string? workspaceValue))
            {
                workspace = workspaceValue;
                return true;
            }

            workspace = null;
            bool doesContainField = false;
            try
            {
                doesContainField = ctx.Field.Arguments.ContainsField("workspace");
                if (!doesContainField)
                    return false;

                workspace = ctx.ArgumentValue<string>("workspace");
            }
            catch (GraphQLException)
            {
                // now HotChocolate throws exception instead of null
                workspace = null;
            }

            return doesContainField;
        }

        private bool HasUserAccessToWorkspace(IMiddlewareContext context, string workspace)
        {
            if (TryGetAuthenticatedPrincipal(context, out var principal))
            {
                if (principal.Identity == null || !principal.Identity.IsAuthenticated)
                    return false;
                
                if (principal.IsInRole(UserRoles.Administrator.ToString()))
                    return true;

                if (principal.HasClaim(WorkspaceConstants.ClaimType, workspace))
                    return true;

                return false;
            }

            return true;
        }

        private static bool TryGetAuthenticatedPrincipal(
            IMiddlewareContext context,
            [NotNullWhen(true)]out ClaimsPrincipal? principal)
        {
            if (context.ContextData.TryGetValue(nameof(ClaimsPrincipal), out var o)
                && o is ClaimsPrincipal p
                && p.Identities.Any(t => t.IsAuthenticated))
            {
                principal = p;
                return true;
            }

            principal = null;
            return false;
        }
    }
}
