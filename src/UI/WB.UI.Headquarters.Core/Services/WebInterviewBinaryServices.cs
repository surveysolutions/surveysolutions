using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Services
{
    public interface IWebInterviewBinaryServices
    {
        IImageProcessingService ImageProcessingService { get; }
        IAudioFileStorage AudioFileStorage { get; }
        IAudioProcessingService AudioProcessingService { get; }
        IImageFileStorage ImageFileStorage { get; }
    }

    public class WebInterviewBinaryServices : IWebInterviewBinaryServices
    {
        public WebInterviewBinaryServices(
            IImageProcessingService imageProcessingService,
            IAudioFileStorage audioFileStorage,
            IAudioProcessingService audioProcessingService,
            IImageFileStorage imageFileStorage)
        {
            ImageProcessingService = imageProcessingService;
            AudioFileStorage = audioFileStorage;
            AudioProcessingService = audioProcessingService;
            ImageFileStorage = imageFileStorage;
        }

        public IImageProcessingService ImageProcessingService { get; }
        public IAudioFileStorage AudioFileStorage { get; }
        public IAudioProcessingService AudioProcessingService { get; }
        public IImageFileStorage ImageFileStorage { get; }
    }
}
