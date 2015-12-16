namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface INetworkService
    {
        bool IsNetworkEnabled();
        bool IsHostReachable(string host);
        string GetNetworkTypeName();
        string GetNetworkName();
    }
}