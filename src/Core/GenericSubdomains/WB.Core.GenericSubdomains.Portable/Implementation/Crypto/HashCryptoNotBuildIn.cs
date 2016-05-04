using System.Diagnostics;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Crypto
{
    internal abstract class BlockHash : Hash, IBlockHash
    {
        protected readonly HashBuffer m_buffer;
        protected ulong m_processed_bytes;

        protected BlockHash(int a_hash_size, int a_block_size, int a_buffer_size = -1) 
            : base(a_hash_size, a_block_size)
        {
            if (a_buffer_size == -1)
                a_buffer_size = a_block_size;

            this.m_buffer = new HashBuffer(a_buffer_size);
            this.m_processed_bytes = 0;
        }

        public override void TransformBytes(byte[] a_data, int a_index, int a_length)
        {
            Debug.Assert(a_index >= 0);
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_index + a_length <= a_data.Length);

            if (!this.m_buffer.IsEmpty)
            {
                if (this.m_buffer.Feed(a_data, ref a_index, ref a_length, ref this.m_processed_bytes))
                    this.TransformBuffer();
            }

            while (a_length >= this.m_buffer.Length)
            {
                this.m_processed_bytes += (ulong)this.m_buffer.Length;
                this.TransformBlock(a_data, a_index);
                a_index += this.m_buffer.Length;
                a_length -= this.m_buffer.Length;
            }

            if (a_length > 0)
                this.m_buffer.Feed(a_data, ref a_index, ref a_length, ref this.m_processed_bytes);
        }

        public override void Initialize()
        {
            this.m_buffer.Initialize();
            this.m_processed_bytes = 0;
        }

        public override HashResult TransformFinal()
        {
            this.Finish();

            Debug.Assert(this.m_buffer.IsEmpty);

            byte[] result = this.GetResult();

            Debug.Assert(result.Length == this.HashSize);

            this.Initialize();
            return new HashResult(result);
        }

        protected void TransformBuffer()
        {
            Debug.Assert(this.m_buffer.IsFull);

            this.TransformBlock(this.m_buffer.GetBytes(), 0);
        }

        protected abstract void Finish();
        protected abstract void TransformBlock(byte[] a_data, int a_index);
        protected abstract byte[] GetResult();
    }
}
