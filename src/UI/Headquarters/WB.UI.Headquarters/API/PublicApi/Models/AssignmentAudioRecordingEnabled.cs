using System.Runtime.Serialization;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class AssignmentAudioRecordingEnabled
    {
        /// <summary>
        /// Enabled for assignment. If null related questionnaire setting is used
        /// </summary>
        [DataMember(IsRequired = true)]
        public bool Enabled { get; set; }
    }
}
