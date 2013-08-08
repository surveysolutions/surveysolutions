namespace WB.Core.BoundedContexts.Supervisor
{
    public interface ITemporaryDataRepositoryAccessor
    {
        void Store<T>(T payload, string name) where T : class;
        T GetByName<T>(string name) where T :class;
    }
}