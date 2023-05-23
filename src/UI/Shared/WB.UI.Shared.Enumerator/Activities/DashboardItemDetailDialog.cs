#nullable enable
using Android.Animation;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using AndroidX.CardView.Widget;
using AndroidX.Core.App;
using MvvmCross;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.Platforms.Android.Views;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities;

[MvxDialogFragmentPresentation()]
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

    /*public override void OnCreate(Bundle bundle)
    {
        this.SetStyle(0, Resource.Style.DialogAnimationTheme);
        base.OnCreate(bundle);
    }*/

    public override Dialog OnCreateDialog(Bundle? savedInstanceState)
    {
        //return base.OnCreateDialog(savedInstanceState);
        if (this.Context == null)
            throw new InvalidOperationException("Context is null");
        
        Dialog dialog = new Dialog(this.Context/*, Resource.Style.DialogAnimationTheme*/);
        dialog.Window!.RequestFeature(WindowFeatures.NoTitle);
        dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
        dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        return dialog;
    }

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
        //this.Dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
        this.Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        //this.Dialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);
        this.Dialog.SetCancelable(true);
        this.Dialog.SetCanceledOnTouchOutside(true);

        return this.BindingInflate(ViewResourceId, container, false);
    }

    public override void OnViewCreated(View view, Bundle? savedInstanceState)
    {
        base.OnViewCreated(view, savedInstanceState);
        
        int[] cardLocation = new int[2];
        CardView cardViewDetails = this.View!.FindViewById<CardView>(Resource.Id.dashboardItem)!;
        /*cardView.GetLocationOnScreen(cardLocation);
        int cardX = cardLocation[0];
        int cardY = cardLocation[1];

        cardView.PivotX = 0;
        cardView.PivotY = 0;
        cardView.ScaleX = 1;
        cardView.ScaleY = 1;
        cardView.TranslationX = 0;
        cardView.TranslationY = 0;

        AnimatorSet animatorSet = new AnimatorSet();
        animatorSet.Play(ObjectAnimator.OfFloat(cardView, View.X, cardX, 0))!
            .With(ObjectAnimator.OfFloat(cardView, View.Y, cardY, 0))!
            .With(ObjectAnimator.OfFloat(cardView, View.ScaleXs, 1f, 1.2f))!
            .With(ObjectAnimator.OfFloat(cardView, View.ScaleYs, 1f, 1.2f));
        animatorSet.SetDuration(3000);
        animatorSet.SetInterpolator(new AccelerateDecelerateInterpolator());
        animatorSet.Start();*/
        

        cardViewDetails.TranslationX = 500;
        cardViewDetails.TranslationY = 100;
        cardViewDetails.Animate()!
            .TranslationX(0)
            .TranslationY(0)
            .ScaleX(1.2f)
            .ScaleY(1.2f)
            .SetDuration(3000)
            .SetInterpolator(new AccelerateDecelerateInterpolator())
            .Start();
    }
}