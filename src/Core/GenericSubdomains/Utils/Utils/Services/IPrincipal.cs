namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IPrincipal
    {
        IIdentity CurrentIdentity { get; }
        void SignIn(string userName, string password, bool rememberMe);
        void SignOut();
    }
}