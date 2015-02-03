﻿using System.IO;
using Machine.Specifications;
using Microsoft.Isam.Esent.Collections.Generic;
using WB.Core.Infrastructure.Storage.Esent.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.CleanIntegration.EsentTests
{
    internal class with_esent_store<T> where T : class, IReadSideRepositoryEntity
    {
        Establish context = () =>
        {
            storePath = Path.GetTempPath();
            storage = new EsentKeyValueStorage<T>(new EsentSettings(storePath));
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