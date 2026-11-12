using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Android.OS;
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
        private const int ExifOrientationNormal = 1;
        private const int ExifOrientationFlipHorizontal = 2;
        private const int ExifOrientationRotate180 = 3;
        private const int ExifOrientationFlipVertical = 4;
        private const int ExifOrientationTranspose = 5;
        private const int ExifOrientationRotate90 = 6;
        private const int ExifOrientationTransverse = 7;
        private const int ExifOrientationRotate270 = 8;

        private readonly IPermissionsService permissions;

        public PictureChooser(IPermissionsService permissions)
        {
            this.permissions = permissions;
        }

        public async Task<Stream> TakePicture()
        {
            FileResult photo = null;
            try
            {
                await this.permissions.AssureHasPermissionOrThrow<Permissions.Camera>();
                if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                    await this.permissions.AssureHasExternalStoragePermissionOrThrow();
                photo = await MediaPicker.CapturePhotoAsync().ConfigureAwait(false);
            }
            catch (PermissionException e)
            {
                throw new MissingPermissionsException(e.Message, e);
            }

            if (photo == null)
                return null;

            try
            {
                try
                {
                    return ProcessCapturedPhoto(photo.FullPath);
                }
                catch (PictureProcessingException)
                {
                    throw;
                }
                catch (System.Exception e)
                {
                    throw new PictureProcessingException(UIResources.Multimedia_PhotoProcessingFailed, e);
                }
            }
            finally
            {
                try
                {
                    File.Delete(photo.FullPath);
                }
                catch (System.Exception e) when (e is IOException || e is UnauthorizedAccessException)
                {
                    NLog.LogManager.GetCurrentClassLogger().Warn(e, "Unable to delete captured photo");
                }
            }
        }

        internal static Stream ProcessCapturedPhoto(string photoPath)
        {
            using (var originalMetadata = new ExifInterface(photoPath))
            {
                var boundsOptions = new BitmapFactory.Options { InJustDecodeBounds = true };
                BitmapFactory.DecodeFile(photoPath, boundsOptions);
                var sampleSize = BitmapHelper.CalculateInSampleSize(boundsOptions, 1024, 1024);
                using var bitmap = BitmapFactory.DecodeFile(photoPath, new BitmapFactory.Options { InSampleSize = sampleSize });
                if (bitmap == null)
                    throw new PictureProcessingException(UIResources.Multimedia_PhotoProcessingFailed);

                var orientation = originalMetadata.GetAttributeInt(ExifInterface.TagOrientation, ExifOrientationNormal);
                var processedBitmap = bitmap;
                if (orientation != ExifOrientationNormal)
                {
                    using var matrix = GetOrientationMatrix(orientation);
                    processedBitmap = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
                }

                try
                {
                    if (Math.Max(processedBitmap.Width, processedBitmap.Height) > 1024)
                    {
                        var scale = 1024.0 / Math.Max(processedBitmap.Width, processedBitmap.Height);
                        var width = (int)(processedBitmap.Width * scale);
                        var height = (int)(processedBitmap.Height * scale);
                        using var resizedBitmap = Bitmap.CreateScaledBitmap(processedBitmap, width, height, true);
                        return CompressBitmap(resizedBitmap);
                    }

                    return CompressBitmap(processedBitmap);
                }
                finally
                {
                    if (processedBitmap != bitmap)
                        processedBitmap.Dispose();
                }
            }
        }

        public async Task<ChoosePictureResult> ChoosePictureGallery()
        {
            FileResult photo = null;
            
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                    await this.permissions.AssureHasExternalStoragePermissionOrThrow();
                photo = await MediaPicker.PickPhotoAsync().ConfigureAwait(false);
            }
            catch (PermissionException e)
            {
                throw new MissingPermissionsException(e.Message, e);
            }

            return photo == null 
                ? null 
                : new ChoosePictureResult(photo.FileName, await photo.OpenReadAsync());
        }

        private static Matrix GetOrientationMatrix(int orientation)
        {
            var matrix = new Matrix();
            switch (orientation)
            {
                case ExifOrientationFlipHorizontal:
                    matrix.SetScale(-1, 1);
                    break;
                case ExifOrientationRotate180:
                    matrix.SetRotate(180);
                    break;
                case ExifOrientationFlipVertical:
                    matrix.SetScale(1, -1);
                    break;
                case ExifOrientationTranspose:
                    matrix.SetRotate(90);
                    matrix.PostScale(-1, 1);
                    break;
                case ExifOrientationRotate90:
                    matrix.SetRotate(90);
                    break;
                case ExifOrientationTransverse:
                    matrix.SetRotate(-90);
                    matrix.PostScale(-1, 1);
                    break;
                case ExifOrientationRotate270:
                    matrix.SetRotate(-90);
                    break;
            }

            return matrix;
        }

        private static Stream CompressBitmap(Bitmap bitmap)
        {
            using var outputStream = new MemoryStream();
            if (!bitmap.Compress(Bitmap.CompressFormat.Jpeg, 70, outputStream))
                throw new PictureProcessingException(UIResources.Multimedia_PhotoProcessingFailed);

            return new MemoryStream(outputStream.ToArray());
        }
    }
}
