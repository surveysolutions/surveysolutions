using System;
using System.Runtime.Serialization;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class WorkspaceApiView
    {
        [DataMember(IsRequired = true)]
        public string Name { get; set; }
        
        [DataMember(IsRequired = true)]
        public string DisplayName { get; set; }
        
        [DataMember]
        public DateTime? DisabledAtUtc { get; set; }
    }
}
