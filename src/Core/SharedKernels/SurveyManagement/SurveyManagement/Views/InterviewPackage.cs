using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views
{
    public class InterviewPackage
    {
        public virtual string Id { get; set; }
        public virtual Guid InterviewId { get; set; }
        public virtual DateTime IncomingDate { get; set; }
        public virtual string PackageContent { get; set; }
    }

    public class BrokenInterviewPackage
    {
        public virtual string Id { get; set; }
        public virtual Guid InterviewId { get; set; }
        public virtual DateTime IncomingDate { get; set; }
        public virtual DateTime ProcessingDate { get; set; }
        public virtual string PackageContent { get; set; }
        public virtual string ExceptionType { get; set; }
        public virtual string ExceptionMessage { get; set; }
        public virtual string ExceptionStackTrace { get; set; }
    }
}