using System;

namespace WB.Core.BoundedContexts.Capi.Synchronization.Views.InterviewMetaInfo
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