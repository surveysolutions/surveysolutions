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
            private int headerSize = 1024;
            private int placeForSize = 4;
            private int position = 0;
            private int lenght = 0;
            private Byte[] headerBuffer;

            internal Byte[] ByteBuffer { get { return this.headerBuffer; } }

            internal Header()
            {
                Init();
            }

            private void Init()
            {
                this.headerBuffer = new byte[headerSize];
                this.headerBuffer[0] = 0x0A;
                this.headerBuffer[1] = 0x0A;
                this.headerBuffer[2] = 0x0A;
                this.headerBuffer[3] = 0x0A;
            }
            internal int HeaderSize()
            {
                return headerSize;
            }
            internal int PlaceForSize()
            {
                return this.placeForSize;
            }

            internal void FormTable(long pos, int size)
            {
                this.headerBuffer = new byte[headerSize];
                //Dictionary<int,int> table = new Dictionary<int, int>();
                //table.Add(pos,size);
                
                byte[] posbytes = BitConverter.GetBytes(pos);
                byte[] sizebytes = BitConverter.GetBytes(size);
                for (int i = 0; i < posbytes.Length; i++)
                    this.headerBuffer[i] = posbytes[i];
                for (int i = 0; i < sizebytes.Length; i++)
                    this.headerBuffer[posbytes.Length + i] = sizebytes[i];
            }



            internal long GetPosition()
            {
                return this.position;
            }
            internal long GetLenght()
            {
                return this.lenght;
            }
        }

        private DriveInfo usbDriver;

        private string fileName = "UsbArchive.capi";

        private Header header = null;

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
        internal void CreateFile()
        {
            var fullName = this.usbDriver.Name + Path.DirectorySeparatorChar + fileName;
            var exists = File.Exists(fullName);
            if (exists) File.Delete(fullName);
            this.header = new Header();
            var space = this.usbDriver.TotalFreeSpace;

            var fileStream = File.Create(fullName, this.header.PlaceForSize(), FileOptions.WriteThrough);
            fileStream.Write(BitConverter.GetBytes(this.header.HeaderSize()), 0,this.header.PlaceForSize());
            fileStream.SetLength(space - 1);
            fileStream.Close();

            //fileStream = File.Open(fullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            //var lastBytePosistion = exists ? fileStream.Length - 1 : space - 1;

            //fileStream.Position = 0;
            //byte[] bytes = BitConverter.GetBytes(this.header.HeaderSize());
            //fileStream.Write(bytes, 0, this.header.PlaceForSize());
            //fileStream.Position = this.header.PlaceForSize()+1;
            //fileStream.Write(this.header.ByteBuffer, 0, this.header.ByteBuffer.Length);

            //fileStream.Position = lastBytePosistion;
            //fileStream.WriteByte(16);
            //fileStream.Close();
        }

        public void InsertPart(byte[] data)
        {
            var fullName = this.usbDriver.Name + Path.DirectorySeparatorChar + fileName;
            var exists = File.Exists(fullName);
            if (this.header == null) this.header = new Header();

            if (!exists)
            {
                CreateFile();
            }
            else
            {
                ReadHeader();
            }
            try
            {
                var fileStream = File.Open(fullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                if (this.header==null)this.header = new Header();
                var position = this.header.GetPosition();
                var lenght = this.header.GetLenght();
                if (position!=0&&lenght!=0)
                {
                   
                    if ((position -this.header.PlaceForSize() + this.header.HeaderSize()+1)>data.Length)
                    {
                        position = this.header.PlaceForSize() + this.header.HeaderSize() + 1;
                    }
                    else
                    {
                        position = position + lenght + 1;
                    }
                }
                else
                {
                     position = this.header.PlaceForSize() + this.header.HeaderSize()+1;
                }
                fileStream.Position = position;
                fileStream.Write(data, 0, data.Length);
                this.header.FormTable(position, data.Length);
                fileStream.Position = this.header.PlaceForSize() + 1;
                fileStream.Write(this.header.ByteBuffer, 0, this.header.ByteBuffer.Length);
                fileStream.Close();
            }
            catch (Exception exception)
            {
                
                throw exception;
            }
            

        }

        private void ReadHeader()
        {
            var fullName = this.usbDriver.Name + Path.DirectorySeparatorChar + fileName;
            var fileStream = File.Open(fullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            byte[] sizebytes = new byte[this.header.PlaceForSize()];
            fileStream.Read(sizebytes, 0, this.header.PlaceForSize());
            int size = BitConverter.ToInt32(sizebytes, 0);
            fileStream.Read(sizebytes, 0, 4);
            int pos = BitConverter.ToInt32(sizebytes, 0);
            fileStream.Read(sizebytes, 0, 4);
            int lenght = BitConverter.ToInt32(sizebytes, 0);
            fileStream.Close();
        }

        internal void UploadFile(bool forceCreation = false)
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

            this.header = new Header();
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
            fileStream.Write(this.header.ByteBuffer, 0, this.header.ByteBuffer.Length);

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
