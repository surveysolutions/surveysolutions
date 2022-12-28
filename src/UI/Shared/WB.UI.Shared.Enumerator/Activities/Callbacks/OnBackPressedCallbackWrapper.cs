using Android.Runtime;
using AndroidX.Activity;

namespace WB.UI.Shared.Enumerator.Activities.Callbacks;

public class OnBackPressedCallbackWrapper : OnBackPressedCallback
{
    private readonly Action action;

    public OnBackPressedCallbackWrapper(Action action) : base(true)
    {
        this.action = action;
    }
    
    public override void HandleOnBackPressed()
    {
        action.Invoke();
    }
}