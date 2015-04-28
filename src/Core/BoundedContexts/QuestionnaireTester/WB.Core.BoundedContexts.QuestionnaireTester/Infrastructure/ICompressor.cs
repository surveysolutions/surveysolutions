using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface ICompressor
    {
        Task<byte[]> DecompressGZipAsync(byte[] payload);
        Task<byte[]> DecompressDeflateAsync(byte[] payload);
    }
}
