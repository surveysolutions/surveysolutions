using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class QuestionnaireApiItem
    {
        public QuestionnaireApiItem(Guid questionnaireId, long version)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, version).ToString();
            this.Version = version;
        }

        [DataMember]
        public string QuestionnaireIdentity { get; init; }

        [DataMember]
        [Required]
        public Guid QuestionnaireId { get; init; }

        [DataMember]
        [Required]
        public long Version { get; init; }

        [DataMember]
        [Required]
        public string Title { get; init; }

        [DataMember]
        [Required]
        public string Variable { get; init; }

        [DataMember]
        [Required]
        public DateTime LastEntryDate { get; init; }

        [DataMember]
        public bool IsAudioRecordingEnabled { get; init; }

        [DataMember]
        public bool WebModeEnabled { get; set; }
    }
}
