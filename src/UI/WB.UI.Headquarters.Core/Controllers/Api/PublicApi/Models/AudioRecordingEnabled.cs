using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class AudioRecordingEnabled
    {
        /// <summary>
        /// Indicates if audio recording is enabled
        /// </summary>
        [DataMember(IsRequired = true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// List of section/group/roster variable names for the selective audio audit scope
        /// </summary>
        [DataMember]
        public List<string> AudioAuditScope { get; set; }
    }
}
