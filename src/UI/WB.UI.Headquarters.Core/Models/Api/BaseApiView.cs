using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WB.UI.Headquarters.Models.Api
{
    public abstract class BaseApiView
    {
        [DataMember]
        public string Order { get; protected set; }

        [DataMember]
        [Required]
        public int Limit { get; protected set; }

        [DataMember]
        [Required]
        public int TotalCount { get; protected set; }

        [DataMember]
        [Required]
        public int Offset { get; protected set; }
    }
}
