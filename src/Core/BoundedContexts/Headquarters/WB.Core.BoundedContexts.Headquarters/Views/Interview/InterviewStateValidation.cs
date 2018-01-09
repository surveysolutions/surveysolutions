using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewStateValidation
    {
        public Guid Id { get; set; }
        public int[] RosterVector { get; set; }
        public int[] Validations { get; set; }
    }
}