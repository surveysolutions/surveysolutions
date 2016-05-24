using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api.Interview
{
    
    public class RosterApiItem
    {
        public RosterApiItem()
        {
            this.Questions = new List<QuestionApiItem>();
            this.Rosters = new List<RosterApiItem>();
        }

        [DataMember]
        public Guid Id { set; get; }
        
        [DataMember]
        public decimal Item { set; get; }

        [DataMember]
        public List<QuestionApiItem> Questions { set; get; }

        [DataMember]
        public List<RosterApiItem> Rosters { set; get; }

        [JsonIgnore]
        public decimal[] RosterVector { set; get; }
    }
}