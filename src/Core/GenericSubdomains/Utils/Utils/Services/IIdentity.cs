namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IIdentity
    {
        bool IsAuthenticated { get; }
        string Name { get; }
        string Password { get; }
    }
}