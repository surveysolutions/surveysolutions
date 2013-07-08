namespace WB.UI.Designer.WebServices
{
    using System;
    using System.IO;
    using System.ServiceModel;

    /// <summary>
    /// The remote file info.
    /// </summary>
    [MessageContract]
    public class RemoteFileInfo : IDisposable
    {
        #region Fields

        /// <summary>
        /// The file byte stream.
        /// </summary>
        [MessageBodyMember]
        public Stream FileByteStream;

        /// <summary>
        /// The file name.
        /// </summary>
        [MessageHeader]
        public string FileName;

        /// <summary>
        /// The length.
        /// </summary>
        [MessageHeader]
        public long Length;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            if (this.FileByteStream != null)
            {
                this.FileByteStream.Close();
                this.FileByteStream = null;
            }
        }

        #endregion
    }
}