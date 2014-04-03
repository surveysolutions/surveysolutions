﻿using System.Runtime.Serialization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.UI.Headquarters.Models.API
{
    public class InterviewApiDetails
    {
        public InterviewApiDetails(InterviewDetailsView interview)
        {
            this.Interview = interview;
        }

        //should be changed to close internal information
        [DataMember]
        public InterviewDetailsView Interview { get; set; }
    }
}