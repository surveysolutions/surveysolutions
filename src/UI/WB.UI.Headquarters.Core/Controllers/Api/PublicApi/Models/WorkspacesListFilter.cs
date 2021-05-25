using System;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class WorkspacesListFilter
    {
        public int Start { get; set; } = 0;

        public int Length { get; set; } = 20;
        
        /// <summary>
        /// Return only workspaces assigned to specified user
        /// </summary>
        public string UserId { get; set; }

        
        /// <summary>
        /// Returned list will also include disabled workspaces
        /// </summary>
        public bool IncludeDisabled { get; set; }
    }
}
