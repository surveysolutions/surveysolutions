namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface IDeviceChangingVerifier
    {
        bool ConfirmDeviceChanging();

        event RequestDeviceChangeCallBack ConfirmDeviceChangeCallback;
    }

    public delegate bool RequestDeviceChangeCallBack(object sender);
}