namespace WB.UI.Capi.Syncronization
{
    public interface IDeviceChangingVerifier
    {
        bool ConfirmDeviceChanging();

        event RequestDeviceChangeCallBack ConfirmDeviceChangeCallback;
    }

    public delegate bool RequestDeviceChangeCallBack(object sender);

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