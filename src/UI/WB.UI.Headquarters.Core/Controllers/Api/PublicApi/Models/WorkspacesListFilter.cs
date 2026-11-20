using System;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class WorkspacesListFilter : DataTableRequest
    {
        public WorkspacesListFilter()
        {
            Start = 0; // keeps the existing default page start (the base default is 0)
        }

        public new int Length { get; set; } = 20;
        
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
