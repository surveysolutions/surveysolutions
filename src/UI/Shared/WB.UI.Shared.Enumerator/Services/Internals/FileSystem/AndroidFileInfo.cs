using System.IO;
using ICSharpCode.SharpZipLib.VirtualFileSystem;

namespace WB.UI.Shared.Enumerator.Services.Internals.FileSystem
{
    internal class AndroidFileInfo : AndroidFileSystemEntryInfo, IFileInfo
    {
        public AndroidFileInfo(FileInfo fInfo)
            : base(fInfo) {}

        protected FileInfo FInfo
        {
            get { return (FileInfo) this.Info; }
        }

        public long Length
        {
            get { return this.FInfo.Length; }
        }
    }
}