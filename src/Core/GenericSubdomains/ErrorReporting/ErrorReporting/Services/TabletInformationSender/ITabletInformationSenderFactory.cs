namespace WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender
{
    public interface ITabletInformationSenderFactory
    {
        ITabletInformationSender CreateTabletInformationSender(string syncAddressPoint, string registrationKeyName, string androidId);
    }
}
