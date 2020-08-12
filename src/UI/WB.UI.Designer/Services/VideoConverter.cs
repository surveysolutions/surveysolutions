using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.UI.Designer.Services
{
    public class VideoConverter : IVideoConverter
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public VideoConverter(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        private static string? ffmpegPath;

        private string ConverterPath
        {
            get
            {
                if (ffmpegPath == null)
                {
                    var files = Directory.GetFiles(hostingEnvironment.ContentRootPath, "ffmpeg.exe", SearchOption.AllDirectories);
                    ffmpegPath = files.FirstOrDefault();
                    if (ffmpegPath == null)
                        throw new ArgumentNullException("Can't found ffmpeg.exe");
                }
                return ffmpegPath;
            }
        }

        public byte[] CreateThumbnail(byte[] videoBytes)
        {
            string tempVideoFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string thumbVideoFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            File.WriteAllBytes(tempVideoFile, videoBytes);
            
            try
            {
                var processInfo = new ProcessStartInfo();
                processInfo.FileName = "\"" + ConverterPath + "\"";
                processInfo.Arguments = $"-ss {5} -i {"\"" + tempVideoFile + "\""} -f image2 -vframes 1 -y {"\"" + thumbVideoFile + "\""}";
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                using (var process = new Process())
                {
                    process.StartInfo = processInfo;
                    process.Start();
                    process.WaitForExit();

                    return File.ReadAllBytes(thumbVideoFile);
                }
            }
            finally
            {
                File.Delete(tempVideoFile);
                File.Delete(thumbVideoFile);
            }
        }
    }
}
