using System.Threading.Tasks;

namespace WB.UI.Headquarters.Services
{
    public interface IAudioProcessingService
    {
        Task<AudioFileInformation> CompressAudioFileAsync(byte[] bytes);
    }
}