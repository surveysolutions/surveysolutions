using System.IO;
using System.Threading.Tasks;
using Plugin.Media.Abstractions;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    internal class PictureChooser : IPictureChooser
    {
        private readonly IMedia media;

        public PictureChooser(IMedia media)
        {
            this.media = media;
        }

        public async Task<Stream> TakePicture()
        {
            await this.media.Initialize().ConfigureAwait(false);
            var storeCameraMediaOptions = new StoreCameraMediaOptions
            {
                CompressionQuality = 95,
                PhotoSize = PhotoSize.Full
            };
            var photo = await this.media.TakePhotoAsync(storeCameraMediaOptions).ConfigureAwait(false);
            return photo?.GetStream();
        }
    }
}