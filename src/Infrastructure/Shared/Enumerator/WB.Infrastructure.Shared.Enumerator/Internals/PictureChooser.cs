using System.IO;
using System.Threading.Tasks;
using Plugin.Media.Abstractions;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    internal class PictureChooser : IPictureChooser
    {
        private readonly IMedia media;
        private readonly IPermissions permissions;

        public PictureChooser(IMedia media, IPermissions permissions)
        {
            this.media = media;
            this.permissions = permissions;
        }

        public async Task<Stream> TakePicture()
        {
            await this.permissions.AssureHasPermission(Permission.Camera);
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