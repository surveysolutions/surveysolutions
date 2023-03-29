using System;
using AndroidX.Activity;

namespace WB.UI.Shared.Enumerator.Activities.Callbacks;

public class OnBackPressedCallbackWrapper : OnBackPressedCallback
{
    private Action action;

    public OnBackPressedCallbackWrapper(Action action) : base(true)
    {
        this.action = action;
    }
    
    public override void HandleOnBackPressed()
    {
        action.Invoke();
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