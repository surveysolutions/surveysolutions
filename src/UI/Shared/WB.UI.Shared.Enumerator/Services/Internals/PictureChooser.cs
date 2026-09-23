using System;
using System.IO;
using System.Threading.Tasks;
using Android.Media;
using MvvmCross.Base;
using Plugin.Media;
using Plugin.Media.Abstractions;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
using Xamarin.Essentials;
using Stream = System.IO.Stream;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    internal class PictureChooser : IPictureChooser
    {
        private readonly IMedia media;
        private readonly IMediaPicker mediaPicker;
        private readonly IPermissionsService permissions;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher;

        public PictureChooser(IMedia media, IPermissionsService permissions, IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher)
            : this(media, permissions, mainThreadAsyncDispatcher, new EssentialsMediaPicker())
        {
        }

        internal PictureChooser(IMedia media, IPermissionsService permissions, IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher, IMediaPicker mediaPicker)
        {
            this.media = media;
            this.permissions = permissions;
            this.mainThreadAsyncDispatcher = mainThreadAsyncDispatcher;
            this.mediaPicker = mediaPicker;
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

            MediaFile photo = null;
            try
            {
                await this.permissions.AssureHasPermissionOrThrow<Permissions.Camera>();
                await this.permissions.AssureHasExternalStoragePermissionOrThrow();
                photo = await this.mediaPicker.CapturePhotoAsync().ConfigureAwait(false);
            }
            catch (PermissionException e)
            {
                throw new MissingPermissionsException(e.Message, e);
            }

            if (photo == null || string.IsNullOrEmpty(photo.FullPath) || !File.Exists(photo.FullPath))
                return null;
            
            //process image
            //using Media plugin
            MediaImplementation androidMedia = new MediaImplementation();
            using (var originalMetadata = new ExifInterface(photo.FullPath))
            {
                var isProcessed = await androidMedia.FixOrientationAndResizeAsync(photo.FullPath, storeCameraMediaOptions, originalMetadata)
                    .ConfigureAwait(false);

                if (!isProcessed)
                    throw new PictureProcessingException(UIResources.Multimedia_PhotoProcessingFailed);
            }

            return await photo.OpenReadAsync();
        }

        public async Task<ChoosePictureResult> ChoosePictureGallery()
        {
            await this.media.Initialize().ConfigureAwait(false);

            MediaFile photo = null;
            
            try
            {
                await this.permissions.AssureHasExternalStoragePermissionOrThrow();
                photo = await this.mediaPicker.PickPhotoAsync().ConfigureAwait(false);
            }
            catch (PermissionException e)
            {
                throw new MissingPermissionsException(e.Message, e);
            }

            return photo == null 
                ? null 
                : new ChoosePictureResult(photo.FileName, await photo.OpenReadAsync());
        }
    }

    internal interface IMediaPicker
    {
        Task<MediaFile> CapturePhotoAsync();
        Task<MediaFile> PickPhotoAsync();
    }

    internal class EssentialsMediaPicker : IMediaPicker
    {
        public async Task<MediaFile> CapturePhotoAsync()
        {
            var result = await MediaPicker.CapturePhotoAsync().ConfigureAwait(false);
            return MediaFile.From(result);
        }

        public async Task<MediaFile> PickPhotoAsync()
        {
            var result = await MediaPicker.PickPhotoAsync().ConfigureAwait(false);
            return MediaFile.From(result);
        }
    }

    internal class MediaFile
    {
        private readonly Func<Task<Stream>> openReadAsync;

        public MediaFile(string fullPath, string fileName, Func<Task<Stream>> openReadAsync)
        {
            this.FullPath = fullPath;
            this.FileName = fileName;
            this.openReadAsync = openReadAsync;
        }

        public string FullPath { get; }
        public string FileName { get; }

        public Task<Stream> OpenReadAsync() => this.openReadAsync();

        public static MediaFile From(FileResult fileResult)
            => fileResult == null
                ? null
                : new MediaFile(fileResult.FullPath, fileResult.FileName, fileResult.OpenReadAsync);
    }
}
