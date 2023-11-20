using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class WorkspaceCreateApiView
    {
        [Required(ErrorMessageResourceType = typeof(Workspaces), 
            ErrorMessageResourceName = nameof(Workspaces.NameRequired))]
        [MaxLength(12)] // if need change then need change TenantName in export
        [RegularExpression("^[-0-9,a-z]+$", ErrorMessageResourceType = typeof(Workspaces), 
            ErrorMessageResourceName = nameof(Workspaces.InvalidName))]
        [UniqueWorkspaceName(ErrorMessageResourceType = typeof(Workspaces), ErrorMessageResourceName = nameof(Workspaces.NameShouldBeUnique))]
        [ValidWorkspaceName(ErrorMessageResourceType = typeof(Workspaces), ErrorMessageResourceName = nameof(Workspaces.NameShouldNotBeOneOfForbidden))]
        [DataMember(IsRequired = true)]
        public string Name { get; set; }
        
        [Required(ErrorMessageResourceType = typeof(Workspaces), ErrorMessageResourceName = nameof(Workspaces.DisplayNameRequired))]
        [DataMember(IsRequired = true)]
        [MaxLength(300)]
        public string DisplayName { get; set; }
    }
}
