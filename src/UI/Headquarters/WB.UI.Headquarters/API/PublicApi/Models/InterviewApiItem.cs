using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class InterviewApiItem
    {
        public InterviewApiItem(Guid interviewId, Guid questionnaireId, long questionnaireVersion, Guid responsibleId,
            string responsibleName, int errorsCount, InterviewStatus status, DateTime lastEntryDate, IEnumerable<InterviewFeaturedQuestion> featuredQuestions)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.ResponsibleId = responsibleId;
            this.ResponsibleName = responsibleName;
            this.ErrorsCount = errorsCount;
            this.Status = status;
            this.LastEntryDate = lastEntryDate;
            this.FeaturedQuestions = featuredQuestions.Select(q => q);
        }

        [DataMember]
        public IEnumerable<InterviewFeaturedQuestion> FeaturedQuestions { get; set; }

        [DataMember]
        [Required]
        public Guid InterviewId { get; set; }

        [DataMember]
        [Required]
        public Guid QuestionnaireId { get; set; }

        [DataMember]
        [Required]
        public long QuestionnaireVersion { set; get; }

        [DataMember]
        [Required]
        public Guid ResponsibleId { get; set; }

        [DataMember]
        [Required]
        public string ResponsibleName { get; set; }

        [DataMember]
        [Required]
        public int ErrorsCount { get; set; }

        [DataMember]
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public InterviewStatus Status { get; set; }
                
        [DataMember]
        [Required]
        public DateTime LastEntryDate { get; set; }
        
    }
}
