using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewStaticText
    {
        public InterviewStaticText()
        {
        }

        public InterviewStaticText(Guid id)
        {
            this.Id = id;
            this.FailedValidationConditions = new List<FailedValidationCondition>();
            this.FailedWarningConditions = new List<FailedValidationCondition>();
            this.IsInvalid = false;
            this.IsEnabled = true;
        }

        public Guid Id { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsInvalid { get; set; }

        public IReadOnlyList<FailedValidationCondition> FailedValidationConditions { get; set; }

        public IReadOnlyList<FailedValidationCondition> FailedWarningConditions { get; set; }
    }
}