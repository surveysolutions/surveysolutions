#nullable enable
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using MvvmCross;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.Platforms.Android.Views;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities;

[MvxDialogFragmentPresentation]
[Register(nameof(DashboardItemDetailDialog))]
public class DashboardItemDetailDialog : MvxDialogFragment<DashboardItemDetailDialogViewModel>
{
    public DashboardItemDetailDialog()
    {
    }

    protected DashboardItemDetailDialog(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }
    
    protected int ViewResourceId => Resource.Layout.dashboard_item_detail_dialog;

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        this.EnsureBindingContextIsSet(inflater);
        this.BindingContext = new MvxAndroidBindingContext(this.Activity,
            new MvxSimpleLayoutInflaterHolder(inflater),
            (this.DataContext as DashboardItemDetailDialogViewModel)?.DashboardViewItem);
        base.OnCreateView(inflater, container, savedInstanceState);

        if (this.Dialog?.Window == null) 
            throw new InvalidOperationException("Dialog is null");
        this.Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
        this.Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
        this.Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        this.Dialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);
        this.Dialog.SetCancelable(true);
        this.Dialog.SetCanceledOnTouchOutside(true);

        return this.BindingInflate(ViewResourceId, container, false);
    }
}