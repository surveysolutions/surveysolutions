using System;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CoverPrefilledEntity : IDisposable
    {
        public DynamicTextViewModel Title { get; set; }
        public string Answer { get; set; }
        public AttachmentViewModel Attachment { get; set; }

        public void Dispose()
        {
            Title?.Dispose();
        }
    }
}
