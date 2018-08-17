using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    [MvxDialogFragmentPresentation]
    public class SelectResponsibleForAssignmentFragment : MvxDialogFragment<SelectResponsibleForAssignmentViewModel>
    {
        protected int ViewResourceId => Resource.Layout.interview_select_responsible_dialog;

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
}
