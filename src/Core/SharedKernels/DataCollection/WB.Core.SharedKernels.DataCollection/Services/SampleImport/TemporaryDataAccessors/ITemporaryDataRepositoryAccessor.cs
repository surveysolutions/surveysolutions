namespace WB.Core.SharedKernels.DataCollection.Services.SampleImport.TemporaryDataAccessors
{
    public interface ITemporaryDataRepositoryAccessor
    {
        void Store<T>(T payload, string name) where T : class;
        T GetByName<T>(string name) where T :class;
    }
}