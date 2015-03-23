using WB.Core.BoundedContexts.Capi.Services;

namespace WB.UI.Capi.Syncronization
{
    public class DeviceChangingVerifier : IDeviceChangingVerifier
    {
        public event RequestDeviceChangeCallBack ConfirmDeviceChangeCallback;

        protected bool OnRequestCredentials()
        {
            var handler = this.ConfirmDeviceChangeCallback;
            if (handler == null)
                return false;
            return handler(this);
        }

        public bool ConfirmDeviceChanging()
        {
            return this.OnRequestCredentials();
        }
    }
}