using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Android.Events
{
    public class ScreenChangedEventArgs : EventArgs
    {
        public ScreenChangedEventArgs(InterviewItemId? screenId)
        {
            this.ScreenId = screenId;
        }
        public InterviewItemId? ScreenId { get; private set; }
    }
}