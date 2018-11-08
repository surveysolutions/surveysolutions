using System.IO;
using WB.Core.SharedKernels.Questionnaire.Services;

namespace WB.UI.Shared.Web.Services
{
    public class VideoConverter : IVideoConverter
    {
        public byte[] CreateThumbnail(string pathToVideo)
        {
            using (var stream = new MemoryStream())
            {
                var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                ffMpeg.GetVideoThumbnail(pathToVideo, stream);

                return stream.ToArray();
            }
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
