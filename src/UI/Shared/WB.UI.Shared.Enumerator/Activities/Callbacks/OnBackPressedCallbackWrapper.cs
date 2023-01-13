using System;
using AndroidX.Activity;

namespace WB.UI.Shared.Enumerator.Activities.Callbacks;

public class OnBackPressedCallbackWrapper : OnBackPressedCallback
{
    private WeakReference<Action> action;

    public OnBackPressedCallbackWrapper(Action action) : base(true)
    {
        this.action = new WeakReference<Action>(action);
    }
    
    public override void HandleOnBackPressed()
    {
        if (action.TryGetTarget(out var weakAction))
            weakAction.Invoke();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Remove();
            action = null;
        }
        
        base.Dispose(disposing);
    }
}