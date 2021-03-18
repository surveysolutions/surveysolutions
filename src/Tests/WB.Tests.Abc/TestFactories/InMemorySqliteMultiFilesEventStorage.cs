using Moq;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Abc.TestFactories
{
    public class InMemorySqliteMultiFilesEventStorage : SqliteMultiFilesEventStorage
    {
        private class FakeEncryptionService : IEncryptionService
        {
            public void GenerateKeys() { }

            public string Encrypt(string textToEncrypt) => textToEncrypt;

            public string Decrypt(string textToDecrypt) => textToDecrypt;

            public byte[] Encrypt(byte[] value) => value;

            public byte[] Decrypt(byte[] value) => value;
        }

        public InMemorySqliteMultiFilesEventStorage(IEnumeratorSettings enumeratorSettings)
            : base(Mock.Of<ILogger>(),
                new SqliteSettings() { InMemoryStorage = true, PathToDatabaseDirectory = "", InterviewsDirectory = ""},
                enumeratorSettings,
                Mock.Of<IFileSystemAccessor>(s => s.IsFileExists(":memory:") == true),
                new EventTypeResolver(typeof(QuestionAnswered).Assembly),
                new FakeEncryptionService(),
                Mock.Of<IWorkspaceAccessor>())
        {
        }
    }
}
