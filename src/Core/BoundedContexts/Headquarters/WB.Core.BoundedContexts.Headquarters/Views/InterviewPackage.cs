using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class InterviewPackage
    {
        public virtual int Id { get; set; }
        public virtual Guid InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual Guid ResponsibleId { get; set; }
        public virtual InterviewStatus InterviewStatus { get; set; }
        public virtual bool IsCensusInterview { get; set; }
        public virtual DateTime IncomingDate { get; set; }
        public virtual string Events { get; set; }
        public virtual int ProcessAttemptsCount { get; set; }
        public virtual bool IsFullEventStream { get; set; }
    }

    public class BrokenInterviewPackage
    {
        public virtual int Id { get; set; }
        public virtual Guid InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual Guid ResponsibleId { get; set; }
        public virtual InterviewStatus InterviewStatus { get; set; }
        public virtual bool IsCensusInterview { get; set; }
        public virtual DateTime IncomingDate { get; set; }
        public virtual string Events { get; set; }
        public virtual DateTime ProcessingDate { get; set; }
        public virtual string ExceptionType { get; set; }
        public virtual string ExceptionMessage { get; set; }
        public virtual string ExceptionStackTrace { get; set; }
        public virtual long PackageSize { get; set; }
        public virtual string InterviewKey { get; set; }
        public virtual int ReprocessAttemptsCount { get; set; }
        public virtual bool IsFullEventStream { get; set; }
    }
}