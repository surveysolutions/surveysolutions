using System;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAudioDialog
    {
        void ShowAndStartRecording(string title);
        event EventHandler OnCanelRecording;
        event EventHandler OnRecorded;
        void StopRecordingAndSaveResult();
    }
}