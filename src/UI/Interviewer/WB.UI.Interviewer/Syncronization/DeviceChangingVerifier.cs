using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.UI.Interviewer.Syncronization
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