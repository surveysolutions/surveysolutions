using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IZipArchiveProtectionService
    {
        void ProtectZipWithPassword(Stream inputZipStream, Stream outputProtectedZipStream, string password);
    }
}
