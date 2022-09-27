using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public interface IMediaAttachment : IDisposable
    {
        string ContentPath { get; set; }

        void Release();
    }
}
