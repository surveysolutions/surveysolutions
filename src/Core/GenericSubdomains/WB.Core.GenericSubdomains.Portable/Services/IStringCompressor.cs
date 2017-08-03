using System.IO;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    /// <summary>
    /// The StringCompressor interface.
    /// </summary>
    public interface IStringCompressor
    {
        Stream Compress(string data);
        string CompressString(string s);
        T DecompressGZip<T>(Stream stream);
        byte[] DecompressGZip(byte[] payload);
        byte[] DecompressDeflate(byte[] payload);
        T DecompressString<T>(string s) where T : class;
    }
}