using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class BrokenInterviewPackageApiView
    {
        public Guid InterviewId { get; set; }
        public string InterviewKey { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public Guid ResponsibleId { get; set; }
        public InterviewStatus InterviewStatus { get; set; }
        public string Events { get; set; }
        public DateTime IncomingDate { get; set; }
        public DateTime ProcessingDate { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }
        public long PackageSize { get; set; }
    }
}
