using System;

namespace CAPI.Android.Core.Model.ViewModel.InterviewMetaInfo
{
    public class InterviewMetaInfoInputModel
    {
        public InterviewMetaInfoInputModel(Guid interviewid)
        {
            InterviewId = interviewid;
        }
        public Guid InterviewId { get; private set; }
    }
}