using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewPackagesServiceTests
{
    [TestOf(typeof(InterviewPackagesService))]
    internal class InterviewPackagesServiceTestsContext
    {
        protected InterviewKey GeneratedInterviewKey = new InterviewKey(5533);

        protected InterviewPackagesService CreateInterviewPackagesService(IPlainStorageAccessor<InterviewPackage> interviewPackageStorage = null,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage = null,
            ILogger logger = null,
            IJsonAllTypesSerializer serializer = null,
            ICommandService commandService = null,
            IInterviewUniqueKeyGenerator uniqueKeyGenerator = null,
            SyncSettings syncSettings = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviews = null)
        {
            return new InterviewPackagesService(
                interviewPackageStorage ?? new TestPlainStorage<InterviewPackage>(),
                brokenInterviewPackageStorage ?? new TestPlainStorage<BrokenInterviewPackage>(),
                logger ?? Mock.Of<ILogger>(), 
                serializer ?? new JsonAllTypesSerializer(),
                commandService ?? Mock.Of<ICommandService>(),
                uniqueKeyGenerator ?? Mock.Of<IInterviewUniqueKeyGenerator>(x => x.Get() == GeneratedInterviewKey),
                syncSettings ?? new SyncSettings(),
                interviews ?? new TestInMemoryWriter<InterviewSummary>(),
                Mock.Of<ITransactionManager>());
        }
    }
}