using Android.Views;

namespace WB.UI.Shared.Extensions.Activities;

public class OnLayoutChangeListener : Java.Lang.Object, View.IOnLayoutChangeListener
{
    private Action action;

    public OnLayoutChangeListener(Action action)
    {
        this.action = action;
    }

    public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight,
        int oldBottom)
    {
        action.Invoke();
    }

    protected override void Dispose(bool disposing)
    {
        action = null;
        base.Dispose(disposing);
    }
}
