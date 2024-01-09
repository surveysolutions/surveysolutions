#nullable enable
using Android.Content;
using Android.Runtime;
using Android.Views;
using MvvmCross;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Views.Fragments;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities;

public abstract class BaseFragmentDialog<T> : MvxDialogFragment<T> where T : MvxViewModel
{
    protected abstract string? Title { get; }
    protected abstract int LayoutFragmentId { get; }

    protected virtual bool IsCancelable => false;
    
    public BaseFragmentDialog()
    {
    }

    protected BaseFragmentDialog(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        if (Context == null) throw new InvalidOperationException("Context is null");
            
        Context.Theme?.ApplyStyle(Resource.Style.DialogWithTitle, true);
        var ignore = base.OnCreateView(inflater, container, savedInstanceState);
        var view = this.BindingInflate(LayoutFragmentId, null);
            
        if (this.Dialog == null) throw new InvalidOperationException("Dialog is null");
        this.Dialog.SetTitle(Title);
        this.Dialog.SetCancelable(true);
        this.Dialog.SetCanceledOnTouchOutside(IsCancelable);
        
        if (this.Dialog.Window != null)
            this.Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        
        if (Activity == null) throw new InvalidOperationException("Activity is null");
        Activity.Window?.SetSoftInputMode(SoftInput.AdjustPan);
        return view;
    }

    public override void OnDismiss(IDialogInterface dialog)
    {
        HideKeyboard();

        base.OnDismiss(dialog);
    }
        
    public override void OnStop()
    {
        HideKeyboard();

        base.OnStop();
    }

    private void HideKeyboard()
    {
        var activity = Mvx.IoCProvider!.Resolve<IMvxAndroidCurrentTopActivity>()!.Activity;

        activity.RemoveFocusFromEditText();
        if(View!= null)
            activity.HideKeyboard(View.WindowToken);
    }
}
