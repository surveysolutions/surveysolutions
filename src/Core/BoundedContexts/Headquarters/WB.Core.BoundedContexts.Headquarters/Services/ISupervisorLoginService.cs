namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ISupervisorLoginService
    {
        bool IsUnique(string login);
        
        bool AreCredentialsValid(string login, string password);
    }
}
