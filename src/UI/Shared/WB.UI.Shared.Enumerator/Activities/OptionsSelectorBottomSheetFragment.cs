using Android.Runtime;
using Android.Views;
using Google.Android.Material.BottomSheet;
using MvvmCross.DroidX.Material;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Views.Fragments;
using MvvmCross.Plugin.Messenger.Subscriptions;
using MvvmCross.WeakSubscription;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities;

[Register("wb.ui.enumerator.activities.OptionsSelectorBottomSheet")]
public class OptionsSelectorBottomSheetFragment: MvxBottomSheetDialogFragment<BottomSheetOptionsSelectorViewModel>
{
    protected int ViewResourceId => Resource.Layout.bottom_sheet_options_select;

    private MvxWeakEventSubscription<MvxRecyclerView> ItemClickSubscription;
    
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        this.EnsureBindingContextIsSet(inflater);
        var view = this.BindingInflate(ViewResourceId, null);
        
        // var mvxRecyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.recyclerView);
        // ItemClickSubscription = mvxRecyclerView.WeakSubscribe(nameof(mvxRecyclerView.Click), this.RecyclerViewClicked);
        
        return view;
    }

    public override Dialog OnCreateDialog(Bundle savedInstanceState)
    {
        return new BottomSheetDialog(Context, Resource.Style.BottomSheetRoundedCorners);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ItemClickSubscription?.Dispose();
            ItemClickSubscription = null;
        }
        base.Dispose(disposing);
    }
}
