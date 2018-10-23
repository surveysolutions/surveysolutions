using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class QuestionnaireApiItem
    {
        public QuestionnaireApiItem(Guid questionnaireId, long version, string title, DateTime lastEntryDate)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, version).ToString();
            this.Version = version;
            this.Title = title;
            this.LastEntryDate = lastEntryDate;
        }

        [DataMember]
        public string QuestionnaireIdentity { get; set; }

        [DataMember]
        [Required]
        public Guid QuestionnaireId { get; set; }

        [DataMember]
        [Required]
        public long Version { get; set; }

        [DataMember]
        [Required]
        public string Title { get; set; }

        [DataMember]
        [Required]
        public DateTime LastEntryDate { get; set; }
        
    }
}
