using System.Collections.Generic;
using System.Collections.ObjectModel;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public interface IDetailsCompositeItem
    {
        CompositeCollection<object> Children { get; }
    }
}