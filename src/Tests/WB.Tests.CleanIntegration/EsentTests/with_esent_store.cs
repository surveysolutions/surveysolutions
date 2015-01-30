using Machine.Specifications;
using Microsoft.Isam.Esent.Collections.Generic;
using WB.Core.Infrastructure.Storage.Esent.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Integration.EsentTests
{
    internal class with_esent_store<T> where T : class, IReadSideRepositoryEntity
    {
        Establish context = () =>
        {
            storage = new EsentKeyValueStorage<T>(new EsentSettings("TempStore"));
        };

        Cleanup things = () =>
        {
            storage.Dispose();
            if (PersistentDictionaryFile.Exists("TempStore"))
            {
                PersistentDictionaryFile.DeleteFiles("TempStore");
            }
        };

        protected static EsentKeyValueStorage<T> storage;
    }
}