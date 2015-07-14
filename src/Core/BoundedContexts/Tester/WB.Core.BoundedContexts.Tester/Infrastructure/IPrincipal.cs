namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface IPrincipal
    {
        bool IsAuthenticated { get; }
        IUserIdentity CurrentUserIdentity { get; }
        void SignIn(string userName, string password, bool rememberMe);
        void SignOut();
    }
}
