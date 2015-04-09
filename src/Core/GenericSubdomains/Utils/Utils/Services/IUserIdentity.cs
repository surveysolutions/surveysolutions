namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IUserIdentity
    {
        bool IsAuthenticated { get; }
        string Name { get; }
        string Password { get; }
    }
}