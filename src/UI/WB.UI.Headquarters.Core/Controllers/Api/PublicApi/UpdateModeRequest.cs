using System.Runtime.Serialization;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    public class UpdateModeRequest
    {
        [DataMember(IsRequired = true)]
        public bool Enabled { get; set; }
    }
}
