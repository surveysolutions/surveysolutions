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
    }
}
