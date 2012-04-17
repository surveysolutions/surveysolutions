using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public static class ImageSourceHelper {
        public static ImageSource GetImageSource(Uri uri) {
            if(uri == null) return null;
            BitmapImage bi = null;
            BackgroundHelper.DoInMainThread(() => {
                try {
                    bi = new BitmapImage(uri);
                } catch {
                    bi = null;
                }
            });
            return bi;
        }
        public static ImageSource GetImageSource(Stream stream) {
            if(stream == null) return null;
            BitmapImage bi = null;
            BackgroundHelper.DoInMainThread(() => {
                try {
                    bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = stream;
                    bi.EndInit();
                } catch {
                    bi = null;
                }

            });
            return bi;
        }
        public static ImageSource GetImageSource(byte[] data) {
            return data == null ? null : GetImageSource(new MemoryStream(data));
        }
        public static ImageSource CreateEmptyImageSource() {
            BitmapImage bi = new BitmapImage();
            MemoryStream ms = new MemoryStream();
            System.Drawing.Bitmap source = new System.Drawing.Bitmap(1, 1);
            source.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }
    }
}
