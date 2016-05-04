using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.BrokenInterviewPackages
{
    public class BrokenInterviewPackageView
    {
        public int Id { get; set; }
        public Guid InterviewId { get; set; }
        public DateTime IncomingDate { get; set; }
        public DateTime ProcessingDate { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }
        public long PackageSize { get; set; }
        public long CompressedPackageSize { get; set; }
    }
}