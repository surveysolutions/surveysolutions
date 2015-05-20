using System;
using System.Diagnostics;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Crypto
{
    internal class HashBuffer 
    {
        private byte[] m_data;
        private int m_pos;

        public HashBuffer(int a_length)
        {
            Debug.Assert(a_length > 0);

            this.m_data = new byte[a_length];

            this.Initialize();
        }

        public void Initialize()
        {
            this.m_pos = 0;
        }

        public byte[] GetBytes()
        {
            Debug.Assert(this.IsFull);

            this.m_pos = 0;
            return this.m_data;
        }

        public byte[] GetBytesZeroPadded()
        {
            Array.Clear(this.m_data, this.m_pos, this.m_data.Length - this.m_pos); 
            this.m_pos = 0;
            return this.m_data;
        }

        public bool Feed(byte[] a_data, ref int a_start_index, ref int a_length, ref ulong a_processed_bytes)
        {
            Debug.Assert(a_start_index >= 0);
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_start_index + a_length <= a_data.Length);
            Debug.Assert(!this.IsFull);

            if (a_data.Length == 0)
                return false;

            if (a_length == 0)
                return false;

            int length = this.m_data.Length - this.m_pos;
            if (length > a_length)
                length = a_length;

            Array.Copy(a_data, a_start_index, this.m_data, this.m_pos, length);

            this.m_pos += length;
            a_start_index += length;
            a_length -= length;
            a_processed_bytes += (ulong)length;

            return this.IsFull;
        }

        public bool Feed(byte[] a_data, int a_length)
        {
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_length <= a_data.Length);
            Debug.Assert(!this.IsFull);

            if (a_data.Length == 0)
                return false;

            if (a_length == 0)
                return false;

            int length = this.m_data.Length - this.m_pos;
            if (length > a_length)
                length = a_length;

            Array.Copy(a_data, 0, this.m_data, this.m_pos, length);

            this.m_pos += length;

            return this.IsFull;
        }

        public bool IsEmpty
        {
            get
            {
                return this.m_pos == 0;
            }
        }

        public int Pos
        {
            get
            {
                return this.m_pos;
            }
        }

        public int Length
        {
            get
            {
                return this.m_data.Length;
            }
        }

        public bool IsFull
        {
            get
            {
                return (this.m_pos == this.m_data.Length);
            }
        }

        public override string ToString()
        {
            return String.Format("HashBuffer, Legth: {0}, Pos: {1}, IsEmpty: {2}", this.Length, this.Pos, this.IsEmpty);
        }
    }
}
