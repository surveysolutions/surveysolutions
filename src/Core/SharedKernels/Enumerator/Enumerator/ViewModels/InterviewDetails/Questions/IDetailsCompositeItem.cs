using System.Collections.Generic;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public interface IDetailsCompositeItem
    {
        IEnumerable<object> Children { get; }
    }
}