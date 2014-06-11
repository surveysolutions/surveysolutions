using System;

namespace WB.Core.BoundedContext.Capi.Synchronization.Views.InterviewMetaInfo
{
    public class InterviewMetaInfoInputModel
    {
        public InterviewMetaInfoInputModel(Guid interviewid)
        {
            this.InterviewId = interviewid;
        }
        public Guid InterviewId { get; private set; }
    }
}