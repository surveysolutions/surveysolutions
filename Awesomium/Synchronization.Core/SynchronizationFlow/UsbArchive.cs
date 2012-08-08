using System;
using System.IO;

namespace Synchronization.Core.SynchronizationFlow
{
    public class UsbFileArchive
    {
        internal class Header
        {
            private const int HeaderSize = 1024;
            private const int SizeHolderWidth = sizeof(int);

            private Byte[] headerBuffer;

            internal Byte[] ByteBuffer { get { return this.headerBuffer; } }

            internal Header()
            {
                Init();
            }

            /// <summary>
            /// Offset of archive data from end of header
            /// </summary>
            internal int ArchivePosition
            {
                get
                {
                    return BitConverter.ToInt32(this.headerBuffer, SizeHolderWidth);
                }
            }

            /// <summary>
            /// Volume of archive data consumed
            /// </summary>
            internal int ArchiveSize
            {
                get
                {
                    return BitConverter.ToInt32(this.headerBuffer, SizeHolderWidth + SizeHolderWidth);
                }
            }

            private void Init()
            {
                this.headerBuffer = new byte[HeaderSize];

                FormatHeader(0, 0);
            }

            /// <summary>
            /// Save info about archive block in the header
            /// </summary>
            /// <param name="pos"></param>
            /// <param name="size"></param>
            internal void FormatHeader(int pos, int size)
            {
                byte[] headerInfo = BitConverter.GetBytes(HeaderSize);
                byte[] posbytes = BitConverter.GetBytes(pos);
                byte[] sizebytes = BitConverter.GetBytes(size);

                System.Diagnostics.Debug.Assert(headerInfo.Length == SizeHolderWidth && posbytes.Length == SizeHolderWidth && sizebytes.Length == SizeHolderWidth);

                int pointer = 0;

                Array.Copy(headerInfo, 0, this.headerBuffer, pointer, headerInfo.Length);

                pointer += headerInfo.Length;

                Array.Copy(posbytes, 0, this.headerBuffer, pointer, posbytes.Length);

                pointer += posbytes.Length;

                Array.Copy(sizebytes, 0, this.headerBuffer, pointer, sizebytes.Length);
            }
        }

        private const string ShortFileName = "UsbArchive";
        private const string FileExt = ".capi";
        private DriveInfo usbDriver;
        private string fileName = null;
   //     private Header header = new Header();
        //private const int MaxSize = int.MaxValue;//504857600;
        private const int MaxSize = 504857600;

        internal UsbFileArchive(string driver)
        {
            var drives = DriveInfo.GetDrives();

            foreach (var d in drives)
            {
                if (d.Name == driver)
                {
                    this.usbDriver = d;
                    this.fileName = CreateFileName(0);

                    break;
                }
            }

            if (this.usbDriver == null)
                throw new Exception(string.Format("USB driver with name {0} not found", driver));
        }

        private string CreateFileName(int chunkNumber)
        {
            return this.usbDriver.Name + Path.DirectorySeparatorChar + ShortFileName + (chunkNumber > 0 ? chunkNumber.ToString() : string.Empty) + FileExt;
        }

        private FileStream PutFile(int chunkNumber, int size)
        {
            var stream = File.Create(CreateFileName(chunkNumber), 1024, FileOptions.WriteThrough);
            stream.Position = size - 1;
            stream.WriteByte(0);

            return stream;
        }

        protected FileStream CreateFile()
        {
          //  var space = this.usbDriver.TotalFreeSpace;
            return File.Create(this.fileName);
            /*  var fileIndex = (int)(this.usbDriver.TotalFreeSpace / MaxSize);
            var fileVolume = (int)(this.usbDriver.TotalFreeSpace % MaxSize);

            FileStream fileStream = null;

            if (fileVolume == 0)
            {
                fileIndex -= 1;
                fileVolume = MaxSize;
            }

            for (; fileIndex >= 0; fileIndex--, fileVolume = MaxSize)
            {
                if (fileStream != null)
                    fileStream.Close();

                if(fileVolume >(int)this.usbDriver.TotalFreeSpace)
                    fileVolume =(int)this.usbDriver.TotalFreeSpace;

                fileStream = PutFile(fileIndex, fileVolume);
            }
            
            return fileStream;*/
        }

        private FileStream ReadHeader()
        {
            var fileStream = File.Open(this.fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
           // fileStream.Read(this.header.ByteBuffer, 0, this.header.ByteBuffer.Length);

            return fileStream;
        }

        public void SaveArchive(byte[] data)
        {

            try
            {
                var fileStream = File.Exists(this.fileName) ? ReadHeader() : CreateFile();

         /*       int newPosition = this.header.ArchivePosition;

                if (newPosition >= data.Length)
                {
                    // put data before existing archive
                    newPosition = 0;
                }
                else
                {
                    // put data after exsitinge archive
                    newPosition += this.header.ArchiveSize;
                }*/
                
                // step 1: write archive
                fileStream.Position = 0;
                fileStream.Write(data, 0, data.Length);

                // step 2: update archive placement info
               // this.header.FormatHeader(newPosition, data.Length);

              /*  fileStream.Position = 0;
                fileStream.Write(this.header.ByteBuffer, 0, this.header.ByteBuffer.Length);*/
                fileStream.Close();
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Load archive from driver
        /// </summary>
        public byte[] LoadArchive()
        {
            byte[] result = null;
            using (var file = File.Open(this.fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                byte[] buffer = new byte[16*1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    result = ms.ToArray();
                }
            }
            return result;
        }

        public string FileName
        {
            get { return this.fileName; }
        }
    }
}
