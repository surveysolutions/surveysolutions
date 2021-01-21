#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class WorkspaceUpdateApiView
    {
        [Required(ErrorMessageResourceType = typeof(Workspaces), ErrorMessageResourceName = nameof(Workspaces.DisplayNameRequired))]
        [DataMember(IsRequired = true)]
        [MaxLength(300)]
        public string? DisplayName { get; set; }
    }
}
