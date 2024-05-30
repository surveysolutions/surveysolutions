using Android.Content;
using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.DroidX.RecyclerView.ItemTemplates;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.CustomControls;

public class ExpandableTemplateSelector : IMvxTemplateSelector
{
    private static readonly Type ParentType = typeof(CompleteGroup);
    private static readonly Type ChildtType = typeof(EntityWithErrorsViewModel);

    public int GetItemViewType(object forItemObject)
    {
        if (forItemObject == null) return -1;

        var typeOfViewModel = forItemObject.GetType();

        if (typeOfViewModel == ParentType)
            return Resource.Layout.interview_complete_group_item;

        if (typeOfViewModel == ChildtType)
            return Resource.Layout.interview_complete_child_item;

        return -1;
    }

    public int GetItemLayoutId(int fromViewType)
    {
        return fromViewType;
    }

    public int ItemTemplateId { get; set; }
}
