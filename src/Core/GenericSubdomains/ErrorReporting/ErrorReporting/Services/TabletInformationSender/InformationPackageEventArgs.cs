using System;

namespace WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender
{
    public class InformationPackageEventArgs : EventArgs
    {
        public InformationPackageEventArgs(string filePath, long fileSize)
        {
            this.FilePath = filePath;
            this.FileSize = fileSize;
        }

        public string FilePath { get; private set; }
        public long FileSize { get; private set; }
    }
}