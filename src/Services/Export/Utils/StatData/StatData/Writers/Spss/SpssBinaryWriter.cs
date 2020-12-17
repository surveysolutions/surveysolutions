using System;
using System.IO;

namespace StatData.Writers.Spss
{
    internal class SpssBinaryWriter:BinaryWriter
    {
        public SpssBinaryWriter(Stream stream) : base(stream)
        {
            
        }

        public void WriteInt32(Int32[] data)
        {
            foreach (Int32 t in data)
                Write((Int32)t);
        }

        public void WriteLongStringBytes(byte[] b, int truelen)
        {
            if (truelen <= 255)
            {
                Write(b);
            }
            else
            {
                // Write string data portions
                var pos = 0;
                var c = 0;
                while (pos < truelen)
                {
                    var q = truelen - pos;

                    if (q > 255)
                    {
                        Write(b, pos, 255);
                        pos = pos + 255;
                        Write((byte) 32);
                        c++;
                    }
                    else
                    {
                        Write(b, pos, q);
                        pos = pos + q;
                    }
                }

                // Write residual padding
                if (b.Length - pos - c > 0)
                    Write(b, pos, b.Length - pos - c);
            }
        }
    }
}
