using System;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAudioDialog
    {
        void ShowAndStartRecording(string title, int bitRate);
        event EventHandler OnCanelRecording;
        event EventHandler OnRecorded;
    }
}