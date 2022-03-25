using System.Threading.Tasks;
using Android.Media;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using Xamarin.Essentials;
using StoragePermission = Plugin.Permissions.StoragePermission;
using Stream = System.IO.Stream;

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
            await this.permissions.AssureHasPermissionOrThrow<StoragePermission>().ConfigureAwait(false);
            await this.permissions.AssureHasPermissionOrThrow<CameraPermission>().ConfigureAwait(false);
            await this.media.Initialize().ConfigureAwait(false);
            var storeCameraMediaOptions = new StoreCameraMediaOptions()
            {
                CompressionQuality = 70,
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 1024
            };

            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo == null)
                return null;
            
            //process image
            //using Media plugin
            MediaImplementation androidMedia = new MediaImplementation();
            var originalMetadata = new ExifInterface(photo.FullPath);
            androidMedia.FixOrientationAndResizeAsync(photo.FullPath, storeCameraMediaOptions, originalMetadata)
                .WaitAndUnwrapException();
            
            originalMetadata?.Dispose();
            
            return await photo.OpenReadAsync();
        }

        public async Task<Stream> ChoosePictureGallery()
        {
            await this.permissions.AssureHasPermissionOrThrow<StoragePermission>().ConfigureAwait(false);
            await this.permissions.AssureHasPermissionOrThrow<CameraPermission>().ConfigureAwait(false);
            await this.media.Initialize().ConfigureAwait(false);

            var photo = await MediaPicker.PickPhotoAsync();
            return photo == null ? null : await photo.OpenReadAsync();
        }
    }
}
