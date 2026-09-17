using System.IO;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Base;
using NUnit.Framework;
using Plugin.Media.Abstractions;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using Xamarin.Essentials;

namespace WB.Tests.Android.Instrumentation.CustomServices
{
    [TestFixture]
    [TestOf(typeof(PictureChooser))]
    public class PictureChooserTests
    {
        [Test]
        public async Task when_capture_photo_returns_empty_path_should_return_null()
        {
            var media = CreateMediaMock();
            var permissions = CreatePermissionsServiceMock();
            var mediaPicker = new Mock<IMediaPicker>();

            mediaPicker.Setup(x => x.CapturePhotoAsync())
                .ReturnsAsync(new MediaFile("", "photo.jpg", () => Task.FromResult<Stream>(new MemoryStream())));

            var pictureChooser = new PictureChooser(
                media.Object,
                permissions.Object,
                Mock.Of<IMvxMainThreadAsyncDispatcher>(),
                mediaPicker.Object);

            var result = await pictureChooser.TakePicture();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task when_capture_photo_returns_nonexistent_path_should_return_null()
        {
            var media = CreateMediaMock();
            var permissions = CreatePermissionsServiceMock();
            var mediaPicker = new Mock<IMediaPicker>();

            mediaPicker.Setup(x => x.CapturePhotoAsync())
                .ReturnsAsync(new MediaFile("/tmp/non-existing-file.jpg", "photo.jpg", () => Task.FromResult<Stream>(new MemoryStream())));

            var pictureChooser = new PictureChooser(
                media.Object,
                permissions.Object,
                Mock.Of<IMvxMainThreadAsyncDispatcher>(),
                mediaPicker.Object);

            var result = await pictureChooser.TakePicture();

            Assert.That(result, Is.Null);
        }

        private static Mock<IMedia> CreateMediaMock()
        {
            var media = new Mock<IMedia>();
            media.Setup(x => x.Initialize()).Returns(Task.CompletedTask);
            return media;
        }

        private static Mock<IPermissionsService> CreatePermissionsServiceMock()
        {
            var permissions = new Mock<IPermissionsService>();
            permissions.Setup(x => x.AssureHasPermissionOrThrow<Permissions.Camera>()).Returns(Task.CompletedTask);
            permissions.Setup(x => x.AssureHasExternalStoragePermissionOrThrow()).Returns(Task.CompletedTask);
            return permissions;
        }
    }
}
