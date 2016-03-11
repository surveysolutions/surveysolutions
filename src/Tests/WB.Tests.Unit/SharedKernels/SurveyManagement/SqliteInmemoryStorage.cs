using System.IO;
using NSubstitute;
using SQLite.Net;
using SQLite.Net.Platform.Win32;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    internal class SqliteInmemoryStorage<TEntity> : SqlitePlainStorage<TEntity> where TEntity : class , IPlainStorageEntity
    {
        public SqliteInmemoryStorage() : base(new SQLiteConnectionWithLock(new SQLitePlatformWin32(),
                new SQLiteConnectionString(":memory:", true, new BlobSerializerDelegate(
                    new NewtonJsonSerializer(new JsonSerializerSettingsFactory()).SerializeToByteArray,
                    (data, type) => new NewtonJsonSerializer(new JsonSerializerSettingsFactory()).DeserializeFromStream(new MemoryStream(data), type),
                    (type) => true))),
            Substitute.For<ILogger>()
            )
        {
        }
    }
}