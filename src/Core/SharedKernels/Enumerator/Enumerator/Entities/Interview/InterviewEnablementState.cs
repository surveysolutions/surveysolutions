using System;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class InterviewEnablementState
    {
        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }
        public bool IsDisabled { get; set; }
    }
}