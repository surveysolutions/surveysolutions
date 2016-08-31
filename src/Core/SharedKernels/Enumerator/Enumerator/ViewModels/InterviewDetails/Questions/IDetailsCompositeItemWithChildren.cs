using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public interface IDetailsCompositeItemWithChildren : IDetailsCompositeItem
    {
        CompositeCollection<object> Children { get; }
    }
}