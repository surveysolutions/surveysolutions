using System;
using System.IO;
using Moq;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests
{
    internal class SyncPackagesProcessorTestContext
    {
        const string AppDataDirectory = "App_Data";
        const string IncomingCapiPackagesDirectoryName = "IncomingData";
        const string IncomingCapiPackagesWithErrorsDirectoryName = "IncomingDataWithErrors";
        const string IncomingCapiPackageFileNameExtension = "sync";

        protected static SyncPackagesProcessor CreateSyncPackagesProcessor(IJsonUtils jsonUtils = null,
            IFileSystemAccessor fileSystemAccessor = null, ICommandService commandService = null, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryStorage=null,
            IStreamableEventStore eventStore = null, IArchiveUtils archiver = null, IIncomingPackagesQueue incomingPackagesQueue = null, IUnhandledPackageStorage unhandledPackageStorage=null)
        {
            return new SyncPackagesProcessor(logger: Mock.Of<ILogger>(),
                syncSettings:
                    new SyncSettings(AppDataDirectory, IncomingCapiPackagesWithErrorsDirectoryName,
                        IncomingCapiPackageFileNameExtension, IncomingCapiPackagesDirectoryName, ""),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                fileSystemAccessor: fileSystemAccessor ?? CreateDefaultFileSystemAccessorMock().Object,
                jsonUtils: jsonUtils ?? Mock.Of<IJsonUtils>(),
                archiver: archiver ?? Mock.Of<IArchiveUtils>(),
                interviewSummaryRepositoryWriter:
                    interviewSummaryStorage ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                incomingPackagesQueue: incomingPackagesQueue ?? Mock.Of<IIncomingPackagesQueue>(),
                unhandledPackageStorage: unhandledPackageStorage??Mock.Of<IUnhandledPackageStorage>())
            {
                EventStore = eventStore ?? Mock.Of<IStreamableEventStore>(),
                EventBus = Mock.Of<IEventDispatcher>()
            };
        }

        protected static string GetPathToSynchItemInErrorFolder(Guid syncItemId)
        {
            return string.Format(@"{0}\{1}\{2}\{3}V-1.{4}",AppDataDirectory, IncomingCapiPackagesDirectoryName,
                IncomingCapiPackagesWithErrorsDirectoryName, syncItemId, IncomingCapiPackageFileNameExtension);
        }

        protected static string GetPathToSynchItemInSyncPackageFolder(Guid syncItemId)
        {
            return string.Format(@"{0}\{1}\{2}.{3}", AppDataDirectory, IncomingCapiPackagesDirectoryName, syncItemId,
                IncomingCapiPackageFileNameExtension);
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
            fileSystemAccessorMock.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            return fileSystemAccessorMock;
        }

        protected static IJsonUtils CreateJsonUtils(SyncItem syncItem = null, InterviewMetaInfo interviewMetaInfo = null)
        {
            return
                Mock.Of<IJsonUtils>(
                    _ =>
                        _.Deserialize<SyncItem>(It.IsAny<string>()) == syncItem &&
                        _.Deserialize<InterviewMetaInfo>(It.IsAny<string>()) == interviewMetaInfo);
        }
    }
}
