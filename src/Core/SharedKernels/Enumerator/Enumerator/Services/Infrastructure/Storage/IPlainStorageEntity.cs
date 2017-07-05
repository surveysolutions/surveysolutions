namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage
{
    public interface IPlainStorageEntity : IPlainStorageEntity<string>
    {
    }

    public interface IPlainStorageEntity<out TKey>
    {
        TKey Id { get; }
    }
}