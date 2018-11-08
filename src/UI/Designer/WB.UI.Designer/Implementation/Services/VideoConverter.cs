using System.IO;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.UI.Designer.Implementation.Services
{
    public class VideoConverter : IVideoConverter
    {

        public byte[] CreateThumbnail(byte[] videoBytes)
        {
            string tempVideoFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            File.WriteAllBytes(tempVideoFile, videoBytes);

            using (var stream = new MemoryStream())
            {
                var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                ffMpeg.GetVideoThumbnail(tempVideoFile, stream);

                File.Delete(tempVideoFile);

                return stream.ToArray();
            }
        }
    }
}
