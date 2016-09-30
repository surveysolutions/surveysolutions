using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IZipArchiveProtectionService
    {
        Stream ProtectZipWithPassword(Stream inputZipStream, string password);
    }
}
