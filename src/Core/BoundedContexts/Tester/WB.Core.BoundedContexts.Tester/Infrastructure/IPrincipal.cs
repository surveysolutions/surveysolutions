namespace WB.Core.BoundedContexts.Tester.Infrastructure
{
    public interface IPrincipal
    {
        bool IsAuthenticated { get; }
        IUserIdentity CurrentUserIdentity { get; }
        void SignIn(string userName, string password, bool rememberMe);
        void SignOut();
    }
}
