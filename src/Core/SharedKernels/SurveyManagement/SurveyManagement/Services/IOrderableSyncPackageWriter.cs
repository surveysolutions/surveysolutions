using System;

using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IOrderableSyncPackageWriter<T> : IReadSideRepositoryWriter<T>, IReadSideRepositoryCleaner, IChacheableRepositoryWriter
        where T : class, IReadSideRepositoryEntity, IIndexedView
    {
        void StoreNextPackage(string counterId, Func<int, T> createSyncPackage);
    }
}