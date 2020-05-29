using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CoverPrefilledEntity : IDisposable
    {
        public DynamicTextViewModel Title { get; set; }
        public string Answer { get; set; }

        public void Dispose()
        {
            Title?.Dispose();
        }
    }
}
