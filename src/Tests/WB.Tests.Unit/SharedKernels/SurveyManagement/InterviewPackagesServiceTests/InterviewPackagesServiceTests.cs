using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    [TestOf(typeof(InterviewPackagesService))]
    internal class InterviewPackagesServiceTests
    {
        protected static InterviewPackagesService CreateInterviewPackagesService(
            IJsonAllTypesSerializer serializer = null,
            IArchiveUtils archiver = null,
            IPlainStorageAccessor<InterviewPackage> interviewPackageStorage = null,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage = null,
            ICommandService commandService = null,
            SyncSettings syncSettings = null)
        {
            return new InterviewPackagesService(
                syncSettings: syncSettings ?? Mock.Of<SyncSettings>(),
                logger: Mock.Of<ILogger>(),
                serializer: serializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                interviewPackageStorage: interviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<InterviewPackage>>(),
                brokenInterviewPackageStorage: brokenInterviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<BrokenInterviewPackage>>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                uniqueKeyGenerator: Mock.Of<IInterviewUniqueKeyGenerator>(),
                interviews: new TestInMemoryWriter<InterviewSummary>(),
                transactionManager: Mock.Of<ITransactionManager>());
        }
    }
}