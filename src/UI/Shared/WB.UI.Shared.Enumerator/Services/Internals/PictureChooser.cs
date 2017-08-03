using System.IO;
using System.Threading.Tasks;
using Plugin.Media.Abstractions;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.UI.Shared.Enumerator.Services.Internals
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
            await this.permissions.AssureHasPermission(Permission.Storage);
            await this.permissions.AssureHasPermission(Permission.Camera);
            await this.media.Initialize().ConfigureAwait(false);
            var storeCameraMediaOptions = new StoreCameraMediaOptions
            {
                CompressionQuality = 70,
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 1024
            };
            var photo = await this.media.TakePhotoAsync(storeCameraMediaOptions).ConfigureAwait(false);
            return photo?.GetStream();
        }

        public async Task<Stream> ChoosePictureGallery()
        {
            await this.permissions.AssureHasPermission(Permission.Storage).ConfigureAwait(false);
            await this.permissions.AssureHasPermission(Permission.Camera).ConfigureAwait(false);
            await this.media.Initialize().ConfigureAwait(false);

            MediaFile result = await this.media.PickPhotoAsync().ConfigureAwait(false);
            return result?.GetStream();
        }
    }
}