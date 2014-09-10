using System;
using System.IO;
using System.ServiceModel;

namespace WB.UI.Designer.Services
{
    [MessageContract]
    public class RemoteFileInfo : IDisposable
    {
        [MessageBodyMember]
        public Stream FileByteStream;

        [MessageHeader]
        public string FileName;

        [MessageHeader]
        public long Length;

        public void Dispose()
        {
            if (this.FileByteStream != null)
            {
                this.FileByteStream.Close();
                this.FileByteStream = null;
            }
        }
    }
}