using System;
using System.IO;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public static class StreamHelper {
        public const int BufferSize = 1024;
        public static MemoryStream CopyToMemoryStream(Stream stream) {
            byte[] buffer;
            if(stream.CanSeek) {
                buffer = new byte[stream.Length];
                int length = stream.Read(buffer, 0, buffer.Length);
                return new MemoryStream(buffer, 0, length);
            }
            MemoryStream memoryStream = new MemoryStream();
            buffer = new byte[BufferSize];
            using(BinaryReader sr = new BinaryReader(stream)) {
                while(true) {
                    int count = sr.Read(buffer, 0, buffer.Length);
                    if(count == 0) break;
                    memoryStream.Write(buffer, 0, count);
                }
            }
            memoryStream.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
        public static string ReadToEndNoClose(Stream stream) {
            MemoryStream copy = CopyToMemoryStream(stream);
            using(StreamReader sr = new StreamReader(copy)) {
                return sr.ReadToEnd();
            }
        }
    }
}
