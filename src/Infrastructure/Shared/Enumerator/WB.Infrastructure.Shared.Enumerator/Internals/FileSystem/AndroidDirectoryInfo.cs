using System.IO;
using ICSharpCode.SharpZipLib.VirtualFileSystem;

namespace WB.Infrastructure.Shared.Enumerator.Internals.FileSystem
{
    internal class AndroidDirectoryInfo : AndroidFileSystemEntryInfo, IDirectoryInfo
    {
        public AndroidDirectoryInfo(FileSystemInfo info)
            : base(info) {}
    }
}