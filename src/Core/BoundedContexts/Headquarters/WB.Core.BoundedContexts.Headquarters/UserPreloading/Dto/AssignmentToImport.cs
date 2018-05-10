using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto
{
    public class AssignmentToImport
    {
        public virtual int Id { get; set; }
        public virtual Guid? Interviewer { get; set; }
        public virtual Guid? Supervisor { get; set; }
        public virtual int? Quantity { get; set; }
        public virtual string Error { get; set; }
        public virtual List<InterviewAnswer> Answers { get; set; }
        public virtual bool Verified { get; set; }
    }
}
