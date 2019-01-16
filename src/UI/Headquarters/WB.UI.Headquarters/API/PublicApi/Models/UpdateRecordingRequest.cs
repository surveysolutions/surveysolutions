using System.Runtime.Serialization;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class UpdateRecordingRequest
    {
        [DataMember(IsRequired = true)]
        public bool Enabled { get; set; }
    }
}