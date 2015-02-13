using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    [Subject(typeof(UserSynchronizationDenormalizer))]
    internal class UserSynchronizationDenormalizerTestsContext
    {
        protected static UserSynchronizationDenormalizer CreateDenormalizer(
             IReadSideRepositoryWriter<UserDocument> users = null,
            IJsonUtils jsonUtils = null,
            IOrderableSyncPackageWriter<UserSyncPackage> userPackageStorageWriter = null)
        {
            var result = new UserSynchronizationDenormalizer(
                users ?? Mock.Of<IReadSideRepositoryWriter<UserDocument>>(),
                jsonUtils ?? Mock.Of<IJsonUtils>(),
                userPackageStorageWriter ?? Mock.Of<IOrderableSyncPackageWriter<UserSyncPackage>>());

            return result;
        }
    }
}