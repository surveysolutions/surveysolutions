using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Client
{
    class UsbFileArchive
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

        private DriveInfo usbDriver;
        private string fileName = null;
        private string dummyFileName = null;
        private Header header = new Header();
        private const long MaxSize = 504857600;
                                    
        internal UsbFileArchive(string driver)
        {
            var drives = DriveInfo.GetDrives();

            foreach (var d in drives)
            {
                if (d.Name == driver)
                {
                    this.usbDriver = d;
                    this.fileName = this.usbDriver.Name + Path.DirectorySeparatorChar + "UsbArchive.capi";
                    this.dummyFileName = this.usbDriver.Name + Path.DirectorySeparatorChar + "dummy";

                    break;
                }
            }

            if (this.usbDriver == null)
                throw new Exception(string.Format("USB driver with name {0} not found", driver));
        }

        public FileStream CreateFile()
        {
            var space = this.usbDriver.TotalFreeSpace;
            var filespace = (space < MaxSize) ? space : MaxSize;
            int i = 0;
            while (space>filespace)
            {
                
                i++;
                var dummyStream = File.Create(this.dummyFileName + i + ".capi", 1024, FileOptions.WriteThrough);
                if (space>2*MaxSize)
                    dummyStream.SetLength(MaxSize);
                else
                    dummyStream.SetLength(space-MaxSize);

                dummyStream.Close();
                space = this.usbDriver.TotalFreeSpace;
            }
            var fileStream = File.Create(this.fileName, this.header.ByteBuffer.Length * 4, FileOptions.WriteThrough);

            fileStream.Write(this.header.ByteBuffer, 0, this.header.ByteBuffer.Length);
            fileStream.SetLength(filespace);

            return fileStream;
        }

        private FileStream ReadHeader()
        {
            var fileStream = File.Open(this.fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            fileStream.Read(this.header.ByteBuffer, 0, this.header.ByteBuffer.Length);

            return fileStream;
        }

        public void SaveArchive(byte[] data)
        {
           
            try
            {
                var fileStream = File.Exists(this.fileName) ? ReadHeader() : CreateFile();

                int newPosition = this.header.ArchivePosition;

                if (newPosition >= data.Length)
                {
                    // put data before existing archive
                    newPosition = 0;
                }
                else
                {
                    // put data after exsitinge archive
                    newPosition += this.header.ArchiveSize;
                }

                // step 1: write archive
                fileStream.Position = newPosition + this.header.ByteBuffer.Length;
                fileStream.Write(data, 0, data.Length);

                // step 2: update archive placement info
                this.header.FormatHeader(newPosition, data.Length);

                fileStream.Position = 0;
                fileStream.Write(this.header.ByteBuffer, 0, this.header.ByteBuffer.Length);
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
        internal byte[] LoadArchive()
        {
            throw new Exception("Not implemented");
        }
    }
}
