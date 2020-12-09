using System;
using WB.Core.BoundedContexts.Headquarters.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class WorkspacesListFilter
    {
        public int Offset { get; set; } = 0;

        public int Limit { get; set; } = 20;
        
        /// <summary>
        /// Return only workspaces assigned to specified user
        /// </summary>
        public Guid? UserId { get; set; }
    }
}
