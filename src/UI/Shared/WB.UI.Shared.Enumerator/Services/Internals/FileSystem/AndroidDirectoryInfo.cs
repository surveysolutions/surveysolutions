using System.IO;
using ICSharpCode.SharpZipLib.VirtualFileSystem;

namespace WB.UI.Shared.Enumerator.Services.Internals.FileSystem
{
    internal class AndroidDirectoryInfo : AndroidFileSystemEntryInfo, IDirectoryInfo
    {
        public AndroidDirectoryInfo(FileSystemInfo info)
            : base(info) {}
    }
}