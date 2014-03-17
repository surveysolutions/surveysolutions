namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ILoginsChecker
    {
        bool IsUnique(string login);
    }
}
