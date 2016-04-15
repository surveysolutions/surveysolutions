using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewStaticText
    {
        public InterviewStaticText()
        {
            this.FailedValidationConditions = new List<FailedValidationCondition>();
            this.IsInvalid = false;
        }

        public InterviewStaticText(Guid id):this()
        {
            this.Id = id;
        }

        public Guid Id { get; set; }

        public bool IsInvalid { get; set; }

        public IReadOnlyList<FailedValidationCondition> FailedValidationConditions { get; set; }

    }
}