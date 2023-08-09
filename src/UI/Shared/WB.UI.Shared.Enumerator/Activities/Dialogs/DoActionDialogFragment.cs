using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Views.Fragments;

namespace WB.UI.Shared.Enumerator.Activities.Dialogs;

public class DoActionDialogFragment : MvxDialogFragment
{
    protected int ViewResourceId => Resource.Layout.do_action_dialog;

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        this.EnsureBindingContextIsSet(inflater);
        base.OnCreateView(inflater, container, savedInstanceState);

        this.Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
        this.Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
        this.Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        this.Dialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);
        this.Dialog.SetCancelable(false);

        return this.BindingInflate(ViewResourceId, container, false);
    }
}
