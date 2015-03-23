using System.IO;

namespace WB.Core.GenericSubdomains.Utils.Services
{
    /// <summary>
    /// The StringCompressor interface.
    /// </summary>
    public interface IStringCompressor
    {
        Stream Compress(string data);
        string CompressString(string s);
        string CompressObject(object s);
        Stream CompressGZip(object data);
        T DecompressGZip<T>(Stream stream);
        T DecompressDeflate<T>(Stream stream);
        byte[] DecompressGZip(byte[] payload);
        byte[] DecompressDeflate(byte[] payload);
        T DecompressString<T>(string s) where T : class;
        string DecompressString(string s);
    }
}