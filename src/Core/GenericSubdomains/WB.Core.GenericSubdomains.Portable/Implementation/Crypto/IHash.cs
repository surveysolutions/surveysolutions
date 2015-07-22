using System.Text;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Crypto
{
    internal interface IHash
    {
        string Name { get; }
        int BlockSize { get; }
        int HashSize { get; }

    
        HashResult ComputeString(string a_data);
        HashResult ComputeString(string a_data, Encoding a_encoding);

        void Initialize();

        void TransformBytes(byte[] a_data);
        void TransformBytes(byte[] a_data, int a_index);
        void TransformBytes(byte[] a_data, int a_index, int a_length);
    }
}
