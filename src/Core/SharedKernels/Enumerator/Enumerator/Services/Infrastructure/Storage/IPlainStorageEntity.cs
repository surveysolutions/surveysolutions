namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage
{
    public interface IPlainStorageEntity
    {
        /// <summary>
        /// Unique autoincremented id by database engine
        /// </summary>
        int OID { get; set; }
        string Id { get; }
    }
}