using System;
using System.IO;
using Machine.Specifications;
using Microsoft.Isam.Esent.Collections.Generic;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Storage.Esent.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.CleanIntegration.EsentTests
{
    internal class with_esent_store<T> where T : class, IReadSideRepositoryEntity
    {
        Establish context = () =>
        {
            storePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().FormatGuid());
            var fileSystemAccessorStub = Mock.Of<IFileSystemAccessor>(_ => _.IsWritePermissionExists(Moq.It.IsAny<string>()) == true);

            storage = new EsentKeyValueStorage<T>(new EsentSettings(storePath), fileSystemAccessorStub);
        };

        Cleanup things = () =>
        {
            storage.Dispose();
            if (PersistentDictionaryFile.Exists(storePath))
            {
                PersistentDictionaryFile.DeleteFiles(storePath);
            }
        };

        protected static EsentKeyValueStorage<T> storage;
        private static string storePath;
    }
}