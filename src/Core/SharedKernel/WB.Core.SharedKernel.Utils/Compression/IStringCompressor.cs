namespace WB.Core.SharedKernel.Utils.Compression
{
    using System.IO;

    /// <summary>
    /// The StringCompressor interface.
    /// </summary>
    public interface IStringCompressor
    {
        Stream Compress(string data);
        string CompressObject(object s);
        T Decompress<T>(Stream stream) where T : class;
        T DecompressString<T>(string s) where T : class;
    }
}