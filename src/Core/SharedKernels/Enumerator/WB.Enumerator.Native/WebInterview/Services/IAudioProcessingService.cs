using System.Threading.Tasks;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public interface IAudioProcessingService
    {
        Task<AudioFileInformation> CompressAudioFileAsync(byte[] bytes, string mimeType);
    }
}
