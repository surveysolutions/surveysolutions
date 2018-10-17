using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CoverPrefilledQuestion : IDisposable
    {
        public DynamicTextViewModel Question { get; set; }
        public string Answer { get; set; }

        public void Dispose()
        {
            Question?.Dispose();
        }
    }
}
