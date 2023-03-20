using System.Threading.Tasks;
using Android.Media;
using Plugin.Media;
using Plugin.Media.Abstractions;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using Xamarin.Essentials;
using Stream = System.IO.Stream;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    internal class PictureChooser : IPictureChooser
    {
        private readonly IMedia media;
        private readonly IPermissionsService permissions;

        public PictureChooser(IMedia media, IPermissionsService permissions)
        {
            this.media = media;
            this.permissions = permissions;
        }

        public async Task<Stream> TakePicture()
        {
            await this.permissions.AssureHasExternalStoragePermissionOrThrow().ConfigureAwait(false);
            await this.permissions.AssureHasPermissionOrThrow<Permissions.Camera>().ConfigureAwait(false);
            await this.media.Initialize().ConfigureAwait(false);
            var storeCameraMediaOptions = new StoreCameraMediaOptions()
            {
                CompressionQuality = 70,
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 1024
            };

            var photo = await MediaPicker.CapturePhotoAsync().ConfigureAwait(false);
            if (photo == null)
                return null;
            
            //process image
            //using Media plugin
            MediaImplementation androidMedia = new MediaImplementation();
            using (var originalMetadata = new ExifInterface(photo.FullPath))
            {
                androidMedia.FixOrientationAndResizeAsync(photo.FullPath, storeCameraMediaOptions, originalMetadata)
                    .WaitAndUnwrapException();    
            }

            return await photo.OpenReadAsync();
        }

        public async Task<Stream> ChoosePictureGallery()
        {
            await this.permissions.AssureHasExternalStoragePermissionOrThrow().ConfigureAwait(false);
            await this.permissions.AssureHasPermissionOrThrow<Permissions.Camera>().ConfigureAwait(false);
            await this.media.Initialize().ConfigureAwait(false);

            var photo = await MediaPicker.PickPhotoAsync().ConfigureAwait(false);
            return photo == null ? null : await photo.OpenReadAsync();
        }
    }
}
