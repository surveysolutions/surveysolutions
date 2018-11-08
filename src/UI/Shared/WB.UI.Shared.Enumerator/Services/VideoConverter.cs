using System.IO;
using Android.Graphics;
using Android.Media;
using WB.Core.SharedKernels.Questionnaire.Services;
using Path = System.IO.Path;

namespace WB.UI.Shared.Enumerator.Services
{
    public class VideoConverter : IVideoConverter
    {
        public byte[] CreateThumbnail(string pathToVideo)
        {
            var retriever = new MediaMetadataRetriever();
            retriever.SetDataSource(pathToVideo);
            var bitmap = retriever.GetFrameAtTime(1);
            if (bitmap == null) return null;

            var stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);

            return stream.ToArray();
        }

        public byte[] CreateThumbnail(byte[] videoBytes)
        {
            string tempVideoFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            File.WriteAllBytes(tempVideoFile, videoBytes);

            var thumbnail = this.CreateThumbnail(tempVideoFile);

            File.Delete(tempVideoFile);

            return thumbnail;
        }
    }
}
