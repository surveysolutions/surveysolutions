using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public abstract class BaseInterviewAnswer
    {
        protected BaseInterviewAnswer()
        {
            this.IsEnabled = true;
            this.IsValid = true;
            this.FailedValidations = new List<FailedValidationCondition>();
        }

        protected BaseInterviewAnswer(Guid id, decimal[] rosterVector)
            : this()
        {
            this.Id = id;
            this.RosterVector = rosterVector;
        }

        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }

        public bool IsValid { get; set; }
        public bool IsEnabled { get; set; }
        public IList<FailedValidationCondition> FailedValidations { get; set; }
        public abstract bool IsAnswered { get; }

        public abstract void RemoveAnswer();
    }
}