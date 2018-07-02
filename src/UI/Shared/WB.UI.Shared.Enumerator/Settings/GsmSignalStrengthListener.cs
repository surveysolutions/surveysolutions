using Android.Telephony;

namespace WB.UI.Shared.Enumerator.Settings
{
    public class GsmSignalStrengthListener : PhoneStateListener
    {
        private readonly TelephonyManager telephonyManager;
        public int SignalStrength { get; private set; }

        public GsmSignalStrengthListener(TelephonyManager telephonyManager)
        {
            this.telephonyManager = telephonyManager;
            this.telephonyManager.Listen(this, PhoneStateListenerFlags.SignalStrengths);
        }

        public override void OnSignalStrengthsChanged(SignalStrength newSignalStrength)
        {
            if (newSignalStrength.IsGsm)
                this.SignalStrength = newSignalStrength.GsmSignalStrength;
        }

        public new void Dispose()
        {
            this.telephonyManager.Listen(this, PhoneStateListenerFlags.None);
            base.Dispose();
        }
    }
}
