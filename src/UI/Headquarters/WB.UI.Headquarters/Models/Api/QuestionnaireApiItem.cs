using System;
using System.Runtime.Serialization;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
{
    public class QuestionnaireApiItem
    {
        public QuestionnaireApiItem(Guid questionnaireId, long version, string title, DateTime lastEntryDate)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Title = title;
            this.LastEntryDate = lastEntryDate;
        }

        [DataMember]
        public Guid QuestionnaireId { get; set; }

        [DataMember]
        public long Version { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public DateTime LastEntryDate { get; set; }
        
    }
}