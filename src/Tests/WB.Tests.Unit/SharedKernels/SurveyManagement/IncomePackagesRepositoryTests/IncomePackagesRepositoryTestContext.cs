using System;
using System.IO;
using Moq;

using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.Core.Synchronization;
using Newtonsoft.Json;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomePackagesRepositoryTests
{
    internal class IncomePackagesRepositoryTestContext
    {
        const string appDataDirectory = "App_Data";
        const string incomingCapiPackagesDirectoryName = "IncomingData";
        const string incomingCapiPackagesWithErrorsDirectoryName = "IncomingDataWithErrors";
        const string incomingCapiPackageFileNameExtension = "sync";

        protected static IncomingPackagesQueue CreateIncomePackagesRepository(IJsonUtils jsonUtils = null,
            IFileSystemAccessor fileSystemAccessor = null, ICommandService commandService = null, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryStorage=null,
            IStreamableEventStore eventStore = null, IArchiveUtils archiver = null)
        {
            return new IncomingPackagesQueue(logger: Mock.Of<ILogger>(),
                syncSettings: new SyncSettings(appDataDirectory, incomingCapiPackagesWithErrorsDirectoryName, incomingCapiPackageFileNameExtension, incomingCapiPackagesDirectoryName, ""),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                fileSystemAccessor: fileSystemAccessor ?? CreateDefaultFileSystemAccessorMock().Object,
                jsonUtils: jsonUtils ?? Mock.Of<IJsonUtils>(),
                archiver: archiver ?? Mock.Of<IArchiveUtils>(),
                interviewSummaryRepositoryWriter: interviewSummaryStorage ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>())
            {
                EventStore = eventStore ?? Mock.Of<IStreamableEventStore>(),
                EventBus = Mock.Of<IEventDispatcher>()
            };
        }

        protected static string GetPathToSynchItemInErrorFolder(Guid syncItemId)
        {
            return string.Format(@"{0}\{1}\{2}\{3}V-1.{4}",appDataDirectory, incomingCapiPackagesDirectoryName,
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

        protected static IJsonUtils CreateJsonUtils(SyncItem syncItem = null, InterviewMetaInfo interviewMetaInfo = null)
        {
            return
                Mock.Of<IJsonUtils>(
                    _ =>
                        _.Deserialize<SyncItem>(Moq.It.IsAny<string>()) == syncItem &&
                        _.Deserialize<InterviewMetaInfo>(Moq.It.IsAny<string>()) == interviewMetaInfo);
        }
    }
}
