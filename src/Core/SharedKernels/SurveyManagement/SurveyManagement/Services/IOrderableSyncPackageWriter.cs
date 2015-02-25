using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IOrderableSyncPackageWriter<TMeta, TContent> : IReadSideRepositoryCleaner, IChacheableRepositoryWriter
        where TMeta : class, IReadSideRepositoryEntity, IOrderableSyncPackage
        where TContent : class, IReadSideRepositoryEntity, ISyncPackage
    {
        void Store(TContent content, TMeta syncPackageMeta, string partialPackageId, string counterId);
    }
}