using System;
using Android.Bluetooth;
using Android.Content;

namespace WB.UI.Shared.Enumerator.OfflineSync.Activities
{
    public class BluetoothReceiver : BroadcastReceiver
    {
        public event EventHandler BluetoothDisabled;

        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;
            if (action == BluetoothAdapter.ActionStateChanged)
            {
                var state = intent.GetIntExtra(BluetoothAdapter.ExtraState, BluetoothAdapter.Error);
                if (state == (int)State.Off)
                {
                    OnBluetoothDisabled();
                }
            }
        }

        protected virtual void OnBluetoothDisabled()
        {
            BluetoothDisabled?.Invoke(this, EventArgs.Empty);
        }
    }
}