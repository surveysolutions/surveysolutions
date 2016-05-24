using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
{
    public class InterviewApiItem
    {
        public InterviewApiItem(Guid interviewId, Guid questionnaireId, long questionnaireVersion, Guid responsibleId,
            string responsibleName, bool hasErrors, string status, string lastEntryDate, IEnumerable<InterviewFeaturedQuestion> featuredQuestions)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.ResponsibleId = responsibleId;
            this.ResponsibleName = responsibleName;
            this.HasErrors = hasErrors;
            this.Status = status;
            this.LastEntryDate = lastEntryDate;
            this.FeaturedQuestions = featuredQuestions.Select(q => q);
        }

        [DataMember]
        public IEnumerable<InterviewFeaturedQuestion> FeaturedQuestions { get; set; }

        [DataMember]
        public Guid InterviewId { get; set; }

        [DataMember]
        public Guid QuestionnaireId { get; set; }

        [DataMember]
        public long QuestionnaireVersion { set; get; }

        [DataMember]
        public Guid ResponsibleId { get; set; }

        [DataMember]
        public string ResponsibleName { get; set; }

        [DataMember]
        public bool HasErrors { get; set; }

        [DataMember]
        public string Status { get; set; }
                
        [DataMember]
        public string LastEntryDate { get; set; }
        
    }
}