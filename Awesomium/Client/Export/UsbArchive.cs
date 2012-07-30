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
            Byte[] headerBuffer = new byte[1024];

            internal Byte[] ByteBuffer { get { return this.headerBuffer; } }

            internal Header()
            {
                Init();
            }

            private void Init()
            {
                this.headerBuffer[0] = 0x0A;
                this.headerBuffer[1] = 0x0A;
                this.headerBuffer[2] = 0x0A;
                this.headerBuffer[3] = 0x0A;
            }
        }

        private DriveInfo usbDriver;

        internal UsbFileArchive(string driver)
        {
            var drives = DriveInfo.GetDrives();

            foreach (var d in drives)
            {
                if (d.Name == driver)
                {
                    this.usbDriver = d;
                    break;
                }
            }
        }
        
        internal void InsertPart(byte[] data)
        {
        }

        internal void UploadFile(string fileName, bool forceCreation = false)
        {
            if (this.usbDriver == null)
                return;

            var fullName = this.usbDriver.Name + Path.DirectorySeparatorChar + fileName;
            var exists = File.Exists(fullName);

            if (exists)
            {
                if (forceCreation)
                {
                    File.Delete(fullName);
                    exists = false;
                }
                else
                {
                }
            }

            var header = new Header();
            var space = this.usbDriver.TotalFreeSpace;

            var fileStream = exists ?
                File.Open(fullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None) :
                File.Create(fullName, 1024, FileOptions.WriteThrough);

            if (!exists)
            {
                fileStream.SetLength(space - 1);
                fileStream.Close();
                return;
            }


            var lastBytePosistion = exists ? fileStream.Length - 1 : space - 1;

            fileStream.Position = 0;
            fileStream.Write(header.ByteBuffer, 0, header.ByteBuffer.Length);

            fileStream.Position = lastBytePosistion;
            fileStream.WriteByte(16);
            fileStream.Close();
        }


        internal void LoadFile(string fileName)
        {
            var fullName = this.usbDriver.Name + Path.DirectorySeparatorChar + fileName;
            if (!File.Exists(fullName))
                return;

        }
    }
}
