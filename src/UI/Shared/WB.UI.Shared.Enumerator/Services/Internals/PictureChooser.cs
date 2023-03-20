using System.Threading.Tasks;
using Android.Media;
using MvvmCross.Base;
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
        private readonly IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher;

        public PictureChooser(IMedia media, IPermissionsService permissions, IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher)
        {
            this.media = media;
            this.permissions = permissions;
            this.mainThreadAsyncDispatcher = mainThreadAsyncDispatcher;
        }

        public async Task<Stream> TakePicture()
        {
            await this.media.Initialize().ConfigureAwait(false);
            var storeCameraMediaOptions = new StoreCameraMediaOptions()
            {
                CompressionQuality = 70,
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 1024
            };

            FileResult photo = null;
            await mainThreadAsyncDispatcher.ExecuteOnMainThreadAsync(async () =>
            {
                photo = await MediaPicker.CapturePhotoAsync().ConfigureAwait(false);
            });

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
            await this.media.Initialize().ConfigureAwait(false);

            FileResult photo = null;
            await mainThreadAsyncDispatcher.ExecuteOnMainThreadAsync(async () =>
            {
                photo = await MediaPicker.PickPhotoAsync().ConfigureAwait(false);
            });

            return photo == null ? null : await photo.OpenReadAsync();
        }
    }
}
