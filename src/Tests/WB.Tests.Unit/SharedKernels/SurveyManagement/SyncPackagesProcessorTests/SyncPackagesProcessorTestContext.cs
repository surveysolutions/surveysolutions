using System;
using System.IO;
using Moq;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;
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
        protected static SyncPackagesProcessor CreateSyncPackagesProcessor(
            ICommandService commandService = null,  IIncomingSyncPackagesQueue incomingSyncPackagesQueue = null, IBrokenSyncPackagesStorage brokenSyncPackagesStorage=null)
        {
            return new SyncPackagesProcessor(logger: Mock.Of<ILogger>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                incomingSyncPackagesQueue: incomingSyncPackagesQueue ?? Mock.Of<IIncomingSyncPackagesQueue>(),
                brokenSyncPackagesStorage: brokenSyncPackagesStorage??Mock.Of<IBrokenSyncPackagesStorage>());
        }

      /*  const string AppDataDirectory = "App_Data";
        const string IncomingCapiPackagesDirectoryName = "IncomingData";
        const string IncomingCapiPackagesWithErrorsDirectoryName = "IncomingDataWithErrors";
        const string IncomingCapiPackageFileNameExtension = "sync";

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
        */
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
