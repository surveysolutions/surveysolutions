namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public enum BatteryStatus
    {
        Charging,
        Discharging,
        Full,
        NotCharging,
        Unknown
    }

    public enum PowerSource
    {
        Battery,
        Ac,
        Usb,
        Wireless,
        Other
    }

    public interface IBattery
    {
        int GetRemainingChargePercent();
        BatteryStatus GetStatus();
        PowerSource GetPowerSource();
    }
}
