#nullable enable

using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public class InterviewBinaryCleanupRecord
    {
        public virtual Guid Id { get; set; }
        public virtual Guid InterviewId { get; set; }
        public virtual string FileName { get; set; } = string.Empty;
        public virtual QuestionType QuestionType { get; set; }
        public virtual DateTime RequestedAt { get; set; }
        public virtual int FailedCount { get; set; }
    }
}
