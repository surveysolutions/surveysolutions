using System.IO;
using Android.App;
using Android.Graphics;
using Android.Media;
using NUnit.Framework;
using WB.UI.Shared.Enumerator.Services.Internals;
using Path = System.IO.Path;

namespace WB.Tests.Android.Instrumentation.CustomServices
{
    [TestFixture]
    public class PictureChooserTests
    {
        private static readonly Color[] MarkerColors = { Color.Red, Color.Green, Color.Blue, Color.Yellow };

        [TestCase(1, 0, 1, 2, 3)]
        [TestCase(2, 1, 0, 3, 2)]
        [TestCase(3, 3, 2, 1, 0)]
        [TestCase(4, 2, 3, 0, 1)]
        [TestCase(5, 0, 2, 1, 3)]
        [TestCase(6, 2, 0, 3, 1)]
        [TestCase(7, 3, 1, 2, 0)]
        [TestCase(8, 1, 3, 0, 2)]
        public void when_exif_orientation_is_applied_should_transform_marker_positions(
            int orientation, int topLeftMarker, int topRightMarker, int bottomLeftMarker, int bottomRightMarker)
        {
            var path = Path.Combine(Application.Context.CacheDir.AbsolutePath, $"{Guid.NewGuid():N}.jpg");
            try
            {
                using (var bitmap = Bitmap.CreateBitmap(120, 80, Bitmap.Config.Argb8888))
                {
                    SetMarker(bitmap, 5, 5, MarkerColors[0]);
                    SetMarker(bitmap, 85, 5, MarkerColors[1]);
                    SetMarker(bitmap, 5, 45, MarkerColors[2]);
                    SetMarker(bitmap, 85, 45, MarkerColors[3]);

                    using var output = new MemoryStream();
                    Assert.That(bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, output), Is.True);
                    File.WriteAllBytes(path, output.ToArray());
                }

                using (var exif = new ExifInterface(path))
                {
                    exif.SetAttribute(ExifInterface.TagOrientation, orientation.ToString());
                    exif.SaveAttributes();
                }

                using var processed = PictureChooser.ProcessCapturedPhoto(path);
                using var decoded = BitmapFactory.DecodeStream(processed);

                var rotated = orientation >= 5;
                Assert.That(decoded.Width, Is.EqualTo(rotated ? 80 : 120));
                Assert.That(decoded.Height, Is.EqualTo(rotated ? 120 : 80));
                AssertMarker(decoded, 1, 1, MarkerColors[topLeftMarker]);
                AssertMarker(decoded, 5, 1, MarkerColors[topRightMarker]);
                AssertMarker(decoded, 1, 5, MarkerColors[bottomLeftMarker]);
                AssertMarker(decoded, 5, 5, MarkerColors[bottomRightMarker]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void when_captured_photo_exceeds_1024px_should_resize_decoded_jpeg_to_1024px_cap()
        {
            var path = Path.Combine(Application.Context.CacheDir.AbsolutePath, $"{Guid.NewGuid():N}.jpg");
            try
            {
                using (var bitmap = Bitmap.CreateBitmap(2048, 1024, Bitmap.Config.Argb8888))
                {
                    bitmap.EraseColor(Color.Blue);
                    using var output = new MemoryStream();
                    Assert.That(bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, output), Is.True);
                    File.WriteAllBytes(path, output.ToArray());
                }

                using var processed = PictureChooser.ProcessCapturedPhoto(path);
                using var decoded = BitmapFactory.DecodeStream(processed);

                Assert.That(Math.Max(decoded.Width, decoded.Height), Is.EqualTo(1024));
                Assert.That(decoded.Width, Is.EqualTo(1024));
                Assert.That(decoded.Height, Is.EqualTo(512));
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static void SetMarker(Bitmap bitmap, int left, int top, Color color)
        {
            for (var x = left; x < left + 30; x++)
            for (var y = top; y < top + 30; y++)
                bitmap.SetPixel(x, y, color);
        }

        private static void AssertMarker(Bitmap bitmap, int horizontalPosition, int verticalPosition, Color expected)
        {
            var x = bitmap.Width * horizontalPosition / 6;
            var y = bitmap.Height * verticalPosition / 6;
            var actual = bitmap.GetPixel(x, y);

            Assert.That(Math.Abs(actual.R - expected.R), Is.LessThan(60));
            Assert.That(Math.Abs(actual.G - expected.G), Is.LessThan(60));
            Assert.That(Math.Abs(actual.B - expected.B), Is.LessThan(60));
        }
    }
}
