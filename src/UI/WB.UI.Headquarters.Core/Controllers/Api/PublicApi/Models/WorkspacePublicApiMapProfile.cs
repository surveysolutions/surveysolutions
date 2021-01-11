#nullable enable
using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class WorkspacePublicApiMapProfile : Profile
    {
        public WorkspacePublicApiMapProfile()
        {
            this.CreateMap<Workspace, WorkspaceApiView>();
            this.CreateMap<WorkspaceApiView, Workspace>();
        }
    }
}
