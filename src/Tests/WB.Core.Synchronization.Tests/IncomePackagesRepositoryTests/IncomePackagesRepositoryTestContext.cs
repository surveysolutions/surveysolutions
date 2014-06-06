using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.Tests.IncomePackagesRepositoryTests
{
    internal class IncomePackagesRepositoryTestContext
    {
        protected static IncomePackagesRepository CreateIncomePackagesRepository(IJsonUtils jsonUtils = null, IFileSystemAccessor fileSystemAccessor = null, ICommandService commandService=null)
        {
            return new IncomePackagesRepository("", Mock.Of<IQueryableReadSideRepositoryWriter<UserDocument>>(), Mock.Of<ILogger>(),
                new SyncSettings(true), commandService ?? Mock.Of<ICommandService>(), jsonUtils ?? Mock.Of<IJsonUtils>(), fileSystemAccessor ?? CreateDefaultFileSystemAccessorMock().Object);
        }

        protected static string GetPathToSynchItemInErrorFolder(Guid syncItemId)
        {
            return string.Format(@"IncomingData\IncomingDataWithErrors\{0}.sync", syncItemId);
        }

        protected static string GetPathToSynchItemInSyncPackageFolder(Guid syncItemId)
        {
            return string.Format(@"IncomingData\{0}.sync", syncItemId);
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
