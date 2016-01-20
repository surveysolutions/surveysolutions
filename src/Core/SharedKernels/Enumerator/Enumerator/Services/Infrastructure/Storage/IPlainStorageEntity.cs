namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage
{
    public interface IPlainStorageEntity
    {
        int OID { get; set; }
        string Id { get; }
    }
}