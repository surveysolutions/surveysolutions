namespace WB.Core.BoundedContexts.Supervisor.Users
{
    public interface IHeadquartersLoginService
    {
        void LoginAndCreateAccount(string login, string password);
    }
}