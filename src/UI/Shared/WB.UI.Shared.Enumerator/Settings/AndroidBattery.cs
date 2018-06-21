using System;
using Android.App;
using Android.Content;
using Android.OS;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using BatteryStatus = WB.Core.SharedKernels.Enumerator.Implementation.Services.BatteryStatus;

namespace WB.UI.Shared.Enumerator.Settings
{
    public class AndroidBattery : IBattery
    {
        public int GetRemainingChargePercent()
        {
            try
            {
                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                {
                    using (var battery = Application.Context.RegisterReceiver(null, filter))
                    {
                        var level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
                        var scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);

                        return (int) Math.Floor(level * 100D / scale);
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Ensure you have android.permission.BATTERY_STATS");
                throw;
            }
        }

        public BatteryStatus GetStatus()
        {
            try
            {
                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                {
                    using (var battery = Application.Context.RegisterReceiver(null, filter))
                    {
                        int status = battery.GetIntExtra(BatteryManager.ExtraStatus, -1);
                        var isCharging = status == (int) BatteryStatus.Charging || status == (int) BatteryStatus.Full;

                        var chargePlug = battery.GetIntExtra(BatteryManager.ExtraPlugged, -1);
                        var usbCharge = chargePlug == (int) BatteryPlugged.Usb;
                        var acCharge = chargePlug == (int) BatteryPlugged.Ac;
                        bool wirelessCharge = false;
                        wirelessCharge = chargePlug == (int) BatteryPlugged.Wireless;

                        isCharging = (usbCharge || acCharge || wirelessCharge);
                        if (isCharging)
                            return BatteryStatus.Charging;

                        switch (status)
                        {
                            case (int) BatteryStatus.Charging:
                                return BatteryStatus.Charging;
                            case (int) BatteryStatus.Discharging:
                                return BatteryStatus.Discharging;
                            case (int) BatteryStatus.Full:
                                return BatteryStatus.Full;
                            case (int) BatteryStatus.NotCharging:
                                return BatteryStatus.NotCharging;
                            default:
                                return BatteryStatus.Unknown;
                        }
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Ensure you have android.permission.BATTERY_STATS");
                throw;
            }

        }

        public PowerSource GetPowerSource()
        {
            try
            {
                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                {
                    using (var battery = Application.Context.RegisterReceiver(null, filter))
                    {
                        int status = battery.GetIntExtra(BatteryManager.ExtraStatus, -1);
                        var isCharging = status == (int) BatteryStatus.Charging || status == (int) BatteryStatus.Full;

                        var chargePlug = battery.GetIntExtra(BatteryManager.ExtraPlugged, -1);
                        var usbCharge = chargePlug == (int) BatteryPlugged.Usb;
                        var acCharge = chargePlug == (int) BatteryPlugged.Ac;

                        bool wirelessCharge = false;
                        wirelessCharge = chargePlug == (int) BatteryPlugged.Wireless;

                        isCharging = (usbCharge || acCharge || wirelessCharge);

                        if (!isCharging)
                            return PowerSource.Battery;
                        else if (usbCharge)
                            return PowerSource.Usb;
                        else if (acCharge)
                            return PowerSource.Ac;
                        else if (wirelessCharge)
                            return PowerSource.Wireless;
                        else
                            return PowerSource.Other;
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Ensure you have android.permission.BATTERY_STATS");
                throw;
            }
        }
    }
}
