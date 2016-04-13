using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    [Subject(typeof(InterviewPackagesService))]
    internal class InterviewPackagesServiceTestsContext
    {
        protected static InterviewPackagesService CreateInterviewPackagesService(
            IJsonAllTypesSerializer serializer = null,
            IArchiveUtils archiver = null, 
            IPlainStorageAccessor<InterviewPackage> interviewPackageStorage = null,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage = null,
            ICommandService commandService = null)
        {
            return new InterviewPackagesService(
                syncSettings: new SyncSettings("hq"), 
                logger: Mock.Of<ILogger>(), 
                serializer: serializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                interviewPackageStorage: interviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<InterviewPackage>>(),
                brokenInterviewPackageStorage: brokenInterviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<BrokenInterviewPackage>>(),
                commandService: commandService ?? Mock.Of<ICommandService>());
        }
    }
}
