using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Raven.Abstractions.FileSystem;
using Raven.Client.Embedded;
using Raven.Client.FileSystem;
using Raven.Database.Config;
using Raven.Database.Server;
using Raven.Server;
using Raven.Tests.Helpers;
using Raven.Tests.Helpers.Util;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Integration.StorageTests.RavenFilesStoreRepositoryAccessorTests
{
    [Subject(typeof(RavenFilesStoreRepositoryAccessor<TestableView>))]
    internal class RavenFilesStoreRepositoryAccessorTestContext
    {
        private static RavenFilesTest ravenFilesTest = new RavenFilesTest();

        protected static RavenFilesStoreRepositoryAccessor<TestableView> CreateRavenFilesStoreRepositoryAccessor()
        {
            return new RavenFilesStoreRepositoryAccessor<TestableView>(Mock.Of<ILogger>(),
                ravenFilesTest.CreateFileStore());
        }
    }

    internal class TestableView : IView
    {
        public int RandomNumber { get; set; }
    }

    internal class RavenFilesTest : RavenFilesTestBase
    {
        public RavenFilesTest():base()
        {
            server = CreateServer(1804);
            server.Url = GetServerUrl(false, server.SystemDatabase.ServerUrl);
        }

        private readonly RavenDbServer server;
        public IFilesStore CreateFileStore()
        {
            var fileSystemName = Guid.NewGuid().FormatGuid();
            var store = new FilesStore
            {
                Url = server.Url,
                DefaultFileSystem = fileSystemName
            };

            store.Initialize(true);

            return store;           
        }
    }
}
