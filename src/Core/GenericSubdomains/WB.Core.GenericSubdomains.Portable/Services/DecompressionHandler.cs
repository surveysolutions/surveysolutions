using System.IO;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface ICompressor
    {
        string EncodingType { get; }
        Task Compress(Stream source, Stream destination);
        Task Decompress(Stream source, Stream destination);
    }
}