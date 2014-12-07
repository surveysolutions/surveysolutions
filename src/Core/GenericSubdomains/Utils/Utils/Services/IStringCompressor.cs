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
        T Decompress<T>(Stream stream) where T : class;
        T DecompressString<T>(string s) where T : class;
        string DecompressString(string s);
    }
}