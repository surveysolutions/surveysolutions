namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IHeadquartersSynchronizer
    {
        void Pull(string login, string password);
    }
}