using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage
{
    public interface IAsyncPlainStorageRemover<TEntity> where TEntity : class, IPlainStorageEntity
    {
        Task DeleteAllAsync();
    }
}