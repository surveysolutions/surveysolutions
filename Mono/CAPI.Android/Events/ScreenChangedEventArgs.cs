using System;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Events
{
    public class ScreenChangedEventArgs : EventArgs
    {
        public ScreenChangedEventArgs(InterviewItemId? screenId)
        {
            ScreenId = screenId;
        }
        public InterviewItemId? ScreenId { get; private set; }
    }
}