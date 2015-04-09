namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IPrincipal
    {
        IUserIdentity CurrentUserIdentity { get; }
        void SignIn(string userName, string password, bool rememberMe);
        void SignOut();
    }
}