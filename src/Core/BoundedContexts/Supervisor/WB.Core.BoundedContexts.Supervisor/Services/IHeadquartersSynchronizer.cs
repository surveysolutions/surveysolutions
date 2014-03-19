namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface IHeadquartersSynchronizer
    {
        void Pull(string login, string password);
    }
}