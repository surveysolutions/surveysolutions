using Android.Content;
using Android.OS;

namespace WB.UI.Shared.Enumerator.Services;

public class ServiceConnection<T> : Java.Lang.Object, IServiceConnection
    where T: Android.App.Service
{
    public T Service { get; private set; }
    private TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

    public void OnServiceConnected(ComponentName name, IBinder service)
    {
        var binder = service as ServiceBinder<T>;
        if (binder != null)
        {
            Service = binder.GetService();
            tcs.SetResult(true);
        }
    }

    public void OnServiceDisconnected(ComponentName name)
    {
        Service = null;
        tcs = new TaskCompletionSource<bool>();
    }

    public Task WaitOnServiceConnected()
    {
        return tcs.Task;
    }
}
