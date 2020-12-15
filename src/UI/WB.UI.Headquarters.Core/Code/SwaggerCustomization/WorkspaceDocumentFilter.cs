#nullable enable
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code.SwaggerCustomization
{
    public class WorkspaceDocumentFilter : IDocumentFilter
    {
        private readonly IInScopeExecutor inScopeExecutor;
        
        public WorkspaceDocumentFilter(
            IInScopeExecutor inScopeExecutor)
        {
            this.inScopeExecutor = inScopeExecutor;
            
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            this.inScopeExecutor.Execute(sl =>
            {
                IWorkspaceContextAccessor workspaceContextAccessor = sl.GetInstance<IWorkspaceContextAccessor>();
                IWorkspacesService workspacesService = sl.GetInstance<IWorkspacesService>();
                var pathBase = workspaceContextAccessor.CurrentWorkspace()?.PathBase ?? string.Empty;

                swaggerDoc.Servers.Clear();

                foreach (var workspace in workspacesService.GetEnabledWorkspaces())
                {
                    swaggerDoc.Servers.Add(new OpenApiServer
                    {
                        Url = new Url(pathBase, workspace.Name, null).ToString()
                    });
                }
            });
        }
    }
}
