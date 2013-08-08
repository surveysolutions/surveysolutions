namespace WB.Core.Infrastructure
{
    public interface ITemporaryDataStorage<T> where T : class
    {
        void Store(T payload, string name);
        T GetByName(string name);
    }
}