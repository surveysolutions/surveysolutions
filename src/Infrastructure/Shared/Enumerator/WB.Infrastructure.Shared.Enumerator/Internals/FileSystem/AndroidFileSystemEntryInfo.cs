using System;
using System.IO;
using ICSharpCode.SharpZipLib.VirtualFileSystem;
using FileAttributes = ICSharpCode.SharpZipLib.VirtualFileSystem.FileAttributes;

namespace WB.Infrastructure.Shared.Enumerator.Internals.FileSystem
{
    internal class AndroidFileSystemEntryInfo : IVfsElement
    {
        protected FileSystemInfo Info;

        public AndroidFileSystemEntryInfo(FileSystemInfo info)
        {
            this.Info = info;
        }

        public string Name
        {
            get { return this.Info.Name; }
        }

        public bool Exists
        {
            get { return this.Info.Exists; }
        }

        public FileAttributes Attributes
        {
            get
            {
                FileAttributes attrs = 0;
                if (this.Info.Attributes.HasFlag(System.IO.FileAttributes.Normal)) attrs |= FileAttributes.Normal;
                if (this.Info.Attributes.HasFlag(System.IO.FileAttributes.ReadOnly)) attrs |= FileAttributes.ReadOnly;
                if (this.Info.Attributes.HasFlag(System.IO.FileAttributes.Hidden)) attrs |= FileAttributes.Hidden;
                if (this.Info.Attributes.HasFlag(System.IO.FileAttributes.Directory)) attrs |= FileAttributes.Directory;
                if (this.Info.Attributes.HasFlag(System.IO.FileAttributes.Archive)) attrs |= FileAttributes.Archive;

                return attrs;
            }
        }

        public DateTime CreationTime
        {
            get { return this.Info.CreationTime; }
        }

        public DateTime LastAccessTime
        {
            get { return this.Info.LastAccessTime; }
        }

        public DateTime LastWriteTime
        {
            get { return this.Info.LastWriteTime; }
        }
    }
}