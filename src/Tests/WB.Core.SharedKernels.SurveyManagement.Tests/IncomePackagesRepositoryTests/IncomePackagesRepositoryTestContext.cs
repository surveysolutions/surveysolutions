using System;
using System.IO;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.IncomePackagesRepositoryTests
{
    internal class IncomePackagesRepositoryTestContext
    {
        const string appDataDirectory = "App_Data";
        const string incomingCapiPackagesDirectoryName = "IncomingData";
        const string incomingCapiPackagesWithErrorsDirectoryName = "IncomingDataWithErrors";
        const string incomingCapiPackageFileNameExtension = "sync";

        protected static IncomePackagesRepository CreateIncomePackagesRepository(IJsonUtils jsonUtils = null,
            IFileSystemAccessor fileSystemAccessor = null, ICommandService commandService = null, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryStorage=null,
            IStreamableEventStore eventStore = null)
        {
            return new IncomePackagesRepository(logger: Mock.Of<ILogger>(),
                syncSettings: new SyncSettings(true, appDataDirectory, incomingCapiPackagesDirectoryName, incomingCapiPackagesWithErrorsDirectoryName, incomingCapiPackageFileNameExtension),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                fileSystemAccessor: fileSystemAccessor ?? CreateDefaultFileSystemAccessorMock().Object,
                jsonUtils: jsonUtils ?? Mock.Of<IJsonUtils>(),
                interviewSummaryRepositoryWriter: interviewSummaryStorage ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(), overrideReceivedEventTimeStamp: false)
            {
                EventStore = eventStore ?? Mock.Of<IStreamableEventStore>(),
                EventBus = Mock.Of<IEventDispatcher>()
            };
        }

        protected static string GetPathToSynchItemInErrorFolder(Guid syncItemId)
        {
            return string.Format(@"{0}\{1}\{2}\{3}.{4}",appDataDirectory, incomingCapiPackagesDirectoryName,
                incomingCapiPackagesWithErrorsDirectoryName, syncItemId, incomingCapiPackageFileNameExtension);
        }

        protected static string GetPathToSynchItemInSyncPackageFolder(Guid syncItemId)
        {
            return string.Format(@"{0}\{1}\{2}.{3}", appDataDirectory, incomingCapiPackagesDirectoryName, syncItemId,
                incomingCapiPackageFileNameExtension);
        }

        protected static string GetSyncItemAsString(SyncItem item)
        {
            return JsonConvert.SerializeObject(item, Formatting.None, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal
            });
        }

        protected static Mock<IFileSystemAccessor> CreateDefaultFileSystemAccessorMock()
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            return fileSystemAccessorMock;
        }
    }
}
