using WB.Core.BoundedContexts.Headquarters.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class WorkspacesListFilter
    {
        public int Offset { get; set; } = 0;

        public int Limit { get; set; } = 20;
    }
}
