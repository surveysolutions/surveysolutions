using System.Diagnostics;
using System.Text;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Crypto
{
    internal abstract class Hash : IHash
    {
        private readonly int m_block_size;
        private readonly int m_hash_size;

        public static int BUFFER_SIZE = 64 * 1024;

        public Hash(int a_hash_size, int a_block_size)
        {
            Debug.Assert((a_block_size > 0) || (a_block_size == -1));
            Debug.Assert(a_hash_size > 0);

            this.m_block_size = a_block_size;
            this.m_hash_size = a_hash_size;
        }

        public virtual string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public virtual int BlockSize
        {
            get
            {
                return this.m_block_size;
            }
        }

        public virtual int HashSize
        {
            get
            {
                return this.m_hash_size;
            }
        }

        public virtual HashResult ComputeString(string a_data)
        {
            return this.ComputeBytes(Converters.ConvertStringToBytes(a_data));
        }

        public virtual HashResult ComputeString(string a_data, Encoding a_encoding)
        {
            return this.ComputeBytes(Converters.ConvertStringToBytes(a_data, a_encoding));
        }

        public virtual HashResult ComputeBytes(byte[] a_data)
        {
            this.Initialize();
            this.TransformBytes(a_data);
            HashResult result = this.TransformFinal();
            this.Initialize();
            return result;
        }
  
        public void TransformBytes(byte[] a_data)
        {
            this.TransformBytes(a_data, 0, a_data.Length);
        }

        public void TransformBytes(byte[] a_data, int a_index)
        {
            Debug.Assert(a_index >= 0);

            int length = a_data.Length - a_index;

            Debug.Assert(length >= 0);

            this.TransformBytes(a_data, a_index, length);
        }

        public abstract void Initialize();
        public abstract void TransformBytes(byte[] a_data, int a_index, int a_length);
        public abstract HashResult TransformFinal();
    }
}
