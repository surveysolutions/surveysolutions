using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto
{
    public class AssignmentToImport
    {
        public AssignmentToImport()
        {
        }

        public virtual int Id { get; set; }
        public virtual Guid? Interviewer { get; set; }
        public virtual Guid? Supervisor { get; set; }
        public virtual int? Quantity { get; set; }
        public virtual string Error { get; set; }
        public virtual List<InterviewAnswer> Answers { get; set; }
        public virtual bool Verified { get; set; }
        public virtual List<string> ProtectedVariables { get; set; }
        public virtual string Email { get; set; }
        public virtual string Password { get; set; }
        public virtual bool? WebMode { set; get; }
        public virtual bool? IsAudioRecordingEnabled { set; get; }
        public virtual Guid? Headquarters { get; set; }
        public virtual string Comments { get; set; }
        public virtual string TargetArea { get; set; }
    }
}
