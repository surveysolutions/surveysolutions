using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class UpdateRecordingRequest
    {
        [DataMember(IsRequired = true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// List of section/group/roster variable names for the selective audio audit scope.
        /// This value cannot be changed through this endpoint; if provided it must match the
        /// current audio audit scope of the assignment.
        /// </summary>
        [DataMember]
        public List<string> AudioAuditScope { get; set; }
    }
}